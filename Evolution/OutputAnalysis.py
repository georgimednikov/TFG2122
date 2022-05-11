import glob
from io import TextIOWrapper
import json
import plotly.express as px

#region Data Visualization
def PieChart(title, names, values):
    fig = px.pie(names=names, values=values, title=title)
    fig.show()

def BarChart(title, axisNames, xNames, yValues):
    fig = px.bar(x=xNames, y=yValues, title=title)
    fig.update_xaxes(title=axisNames[0])
    fig.update_yaxes(title=axisNames[1])
    fig.show()

def LineChart(title, axisNames, xNames, yValues):
    fig = px.line(x=xNames, y=yValues, title=title)
    fig.update_xaxes(title=axisNames[0])
    fig.update_yaxes(title=axisNames[1])
    fig.show()
#endregion

#region Data Processing

def AppendFile_n_Print(data: str, file: TextIOWrapper):
    file.write(f'{data}\n')
    print(data)

# The processed data is returned in the following structures:
#   globalDeathCauses: A tuple where the first value is a dictionary where each key represents the 
#   death type and each value the number of creatures of that species that died, and the second value
#   is the total number of creatures that existed.
#   speciesDeathCauses: A dictionary where keys are the name of a species and the values are a tuple where
#   the first value is the number of creatures of that species and the second value is a dictionary where each
#   key represents the death type and each value the number of creatures of that species that died.
def ProcessData():
    # Get all the species folder in the Output Dir
    speciesList = glob.glob("*/")
    totalCreatures = 0
    # Initialize the global death causes dictionary each species death causes dictionary
    globalDeathCauses = [dict.fromkeys(['Temperature','Attack','Retalliation','Starved','Dehydration','Exhaustion','Poisoned','Longevity','SimulationEnd'], 0), 0]
    speciesDeathCauses = {}
    #
    speciesDamageReception = {}
    #
    # 3 Elements one per diet
    # -> Herbivores (number of deaths due to starvation, total number of deaths), (damage received due to starvation, total damage received)
    # -> Omnivores (number of deaths due to starvation, total number of deaths), (damage received due to starvation, total damage received)
    # -> Carnivores (number of deaths due to starvation, total number of deaths), (damage received due to starvation, total damage received)
    dietInfo = {
        'Omnivore':   ([0]*2, [0]*2),
        'Herbivore':  ([0]*2, [0]*2),
        'Carnivore':  ([0]*2, [0]*2)
    }

    # Get the death events from every creature in every species folder and add it to the pertinent dictionary
    for i in range(len(speciesList)):
        creature_list = glob.glob(f'{speciesList[i]}*.json')
        speciesDeathCauses[speciesList[i]] = (dict.fromkeys(['Temperature','Attack','Retalliation','Starved','Dehydration','Exhaustion','Poisoned','Longevity','SimulationEnd'], 0), len(creature_list))
        globalDeathCauses[1] += len(creature_list)
        
        # [ 'species', (dict [damagetype, damageDone] , totalDamage) ]
        speciesDamageReception[speciesList[i]] = [dict.fromkeys(['Temperature','Attack','Retalliation','Starvation','Dehydration','Exhaustion','Poison'], 0), 0]
        for j in range(len(creature_list)):
            creatureData = json.load(open(creature_list[j]))
            # Birth data, to acces to creature stats
            birthData = [x for x in creatureData if (x['Type'] == 'CreatureBirth')]
            # Death data
            filteredData = [x for x in creatureData if (x['Type'] == 'CreatureDeath')]            
            creatureDiet = birthData[0]['Diet']
            deathtype = filteredData[0]['DeathType']
            globalDeathCauses[0][deathtype] += 1
            speciesDeathCauses[speciesList[i]][0][deathtype] +=1

            # Diet death data
            if deathtype != 'SimulationEnd':
                dietInfo[creatureDiet][0][1] += 1
                if deathtype == 'Starved':
                    dietInfo[creatureDiet][0][0] += 1

            # Damage received data
            filteredData = [x for x in creatureData if (x['Type'] == 'CreatureReceiveDamage')]
            for k in range(len(filteredData)):
                damageType = filteredData[k]['damageType']
                damageDone = filteredData[k]['damage']
                speciesDamageReception[speciesList[i]][0][damageType] += damageDone    # local
                speciesDamageReception[speciesList[i]][1] += damageDone   # total 

                # Diet damage received data
                if damageType == 'Starvation':
                    dietInfo[creatureDiet][1][0] += damageDone
                dietInfo[creatureDiet][1][1] += damageDone

    return globalDeathCauses, speciesDeathCauses, speciesDamageReception, dietInfo

def DictionaryPieChart(title, dictionary: dict):
    PieChart(title, list(dictionary.keys()), list(dictionary.values()))

def SpeciesDeathBarChart(title, speciesDict: dict, deathCause):
    speciesNames = list(speciesDict.keys())
    speciesDeathCausePerc = list()
    for species in speciesNames:
        perc = speciesDict[species][0][deathCause] / speciesDict[species][1] * 100
        speciesDeathCausePerc.append(perc)

    BarChart(title, ['Species', 'Percentage of deaths'], speciesNames, speciesDeathCausePerc)

def ProcessResults(totalCreatures: int, globalDeathCauses: dict, speciesDeathCauses: dict, speciesDamageReception: dict, dietInfo: dict):
    # Open the analysis output file
    # outputFile = open('analysis.txt', 'w')
    # AppendFile_n_Print(f'Total Creatures : {totalCreatures}', outputFile)
    # ProcessDict(globalDeathCauses, totalCreatures, outputFile)
    deathCause = 'Starved'
    SpeciesDeathBarChart(f'Species {deathCause} percentage', speciesDeathCauses, deathCause)
    DictionaryPieChart('Global death causes', globalDeathCauses)

    # for k,v in speciesDeathCauses.items():
    #     AppendFile_n_Print(f'\n{k} Total Creatures : {v[1]}', outputFile)
    #     ProcessDict(v[0], v[1], outputFile)

def ProcessDict(results: dict, numCreatures: float, file: TextIOWrapper):
    deathCauses = {k: results[k] for k in results.keys() - {'SimulationEnd'}}
    aliveCreatures = {k: results[k] for k in results.keys() - deathCauses.keys()}
    AppendFile_n_Print(f'Last Alive : {aliveCreatures["SimulationEnd"]}\nDeathCauses:', file)
    for cause,num in deathCauses.items():
        AppendFile_n_Print(f'\t{cause} : {num / numCreatures * 100}', file)

#endregion

def main():
    globalDeathCauses, speciesDeathCauses, speciesDamageReception, dietInfo = ProcessData()
    ProcessResults(globalDeathCauses[1], globalDeathCauses[0], speciesDeathCauses)  

main()