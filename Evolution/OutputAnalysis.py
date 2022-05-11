import glob
from io import TextIOWrapper
import json
import plotly.express as px
import plotly.graph_objects as pgo
from plotly.subplots import make_subplots

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

def ShowSpeciesBarChart(title, axisNames, speciesDict: dict, deathCause):
    speciesNames = list(speciesDict.keys())
    speciesDeathCausePerc = list()
    for species in speciesNames:
        perc = speciesDict[species][0][deathCause] / speciesDict[species][1] * 100
        speciesDeathCausePerc.append(perc)

    BarChart(title, axisNames, speciesNames, speciesDeathCausePerc)

def ShowGlobalInfo(globalDeathInfo: dict, globalDamageInfo: dict):
    fig = make_subplots(rows=1, cols=2, specs=[[{"type": "pie"}, {"type": "pie"}]],subplot_titles=(
        'Global death causes',
        'Global damage causes'))

    fig.add_trace(pgo.Pie(labels=list(globalDeathInfo.keys()), values=list(globalDeathInfo.values())), 1, 1)
    fig.add_trace(pgo.Pie(labels=list(globalDamageInfo.keys()), values=list(globalDamageInfo.values())), 1, 2)
    fig.show()

def ShowDietInfo(dietInfo: dict):
    keys = list(dietInfo.keys())
    values = list(dietInfo.values())

    fig = make_subplots(rows=1, cols=3, shared_yaxes=True, subplot_titles=(
        f'Starvation caused deaths and damage among {keys[0]}s',
        f'Starvation caused deaths and damage among {keys[1]}s',
        f'Starvation caused deaths and damage among {keys[2]}s'))

    fig.add_trace(pgo.Bar(x=['Deaths', 'Damage'], y=[values[0][0][0]/values[0][0][1]*100, values[0][1][0]/values[0][1][1]*100]), 1, 1)
    fig.add_trace(pgo.Bar(x=['Deaths', 'Damage'], y=[values[1][0][0]/values[1][0][1]*100, values[1][1][0]/values[1][1][1]*100]), 1, 2)
    fig.add_trace(pgo.Bar(x=['Deaths', 'Damage'], y=[values[2][0][0]/values[2][0][1]*100, values[2][1][0]/values[2][1][1]*100]), 1, 3)
    fig.update_yaxes(title='Percentage')
    fig.show()
#endregion

#region Data Processing
def AppendFile_n_Print(data: str, file: TextIOWrapper):
    file.write(f'{data}\n')
    print(data)

def ProcessDict(results: dict, numCreatures: float, file: TextIOWrapper):
    deathCauses = {k: results[k] for k in results.keys() - {'SimulationEnd'}}
    aliveCreatures = {k: results[k] for k in results.keys() - deathCauses.keys()}
    AppendFile_n_Print(f'Last Alive : {aliveCreatures["SimulationEnd"]}\nDeathCauses:', file)
    for cause,num in deathCauses.items():
        AppendFile_n_Print(f'\t{cause} : {num / numCreatures * 100}', file)

# The processed data is returned in the following structures:
#   globalDeathCauses: A tuple where the first value is a dictionary where each key represents the 
#   death type and each value the number of creatures of that species that died, and the second value
#   is the total number of creatures that existed.
#   speciesDeathCauses: A dictionary where keys are the name of a species and the values are a tuple where
#   the first value is the number of creatures of that species and the second value is a dictionary where each
#   key represents the death type and each value the number of creatures of that species that died.
def ProcessData():
    # Get all the species folder in the Output Dir, and the sessionOutput file
    speciesList = glob.glob("*/")
    #sessionOutput = json.load(open("sessionOutput.json"))
    #sessionStartEvent = [x for x in sessionOutput if (x['Type'] == 'SessionStart')][0]
    # Initialize the global death causes dictionary with each species death cause and the species deaths dictionary
    globalDeathCauses = [dict.fromkeys(['Temperature','Attack','Retalliation','Starved','Dehydration','Exhaustion','Poisoned','Longevity','SimulationEnd'], 0), 0]
    speciesDeathCauses = {}
    # Initialize the global damages dictionary  with each damage cause and the species damages dictionary
    globalDamageReception = [dict.fromkeys(['Temperature','Attack','Retalliation','Starvation','Dehydration','Exhaustion','Poison'], 0), 0]
    speciesDamageReception = {}
    # 
    # 3 Elements one per diet
    # -> Herbivores (number of deaths due to starvation, total number of deaths), (damage received due to starvation, total damage received)
    # -> Omnivores (number of deaths due to starvation, total number of deaths), (damage received due to starvation, total damage received)
    # -> Carnivores (number of deaths due to starvation, total number of deaths), (damage received due to starvation, total damage received)
    dietInfo = {
        'Omnivore':   [[0]*2, [0]*2],
        'Herbivore':  [[0]*2, [0]*2],
        'Carnivore':  [[0]*2, [0]*2]
    }
    # Births information 
    # [0] -> Adulthood rate ( adults / total creatures)
    # [1] -> Reproduction ratio (total mating / adults )
    # [2] -> Births and deaths + timestamp list
    globalBirthInfo = [0, 0, []]   
    speciesBirthInfo = {}

    # Get the death events from every creature in every species folder and add it to the pertinent dictionary
    for i in range(len(speciesList)):
        currSpecies = speciesList[i][:-1]   # remove dir\\
        creature_list = glob.glob(f'{speciesList[i]}*.json')
        speciesDeathCauses[currSpecies] = (dict.fromkeys(['Temperature','Attack','Retalliation','Starved','Dehydration','Exhaustion','Poisoned','Longevity','SimulationEnd'], 0), len(creature_list))
        globalDeathCauses[1] += len(creature_list)
        # [ 'species', (dict [damagetype, damageDone] , totalDamage) ]
        speciesDamageReception[currSpecies] = [dict.fromkeys(['Temperature','Attack','Retalliation','Starvation','Dehydration','Exhaustion','Poison'], 0), 0]
        speciesBirthInfo[currSpecies] = [0, 0, []]

        for j in range(len(creature_list)):
            creatureData = json.load(open(creature_list[j]))
            # Birth data, to acces to creature stats
            birthData = [x for x in creatureData if (x['Type'] == 'CreatureBirth')]
            # Death data
            filteredData = [x for x in creatureData if (x['Type'] == 'CreatureDeath')]            
            creatureDiet = birthData[0]['Diet']
            deathtype = filteredData[0]['DeathType']
            globalDeathCauses[0][deathtype] += 1
            speciesDeathCauses[currSpecies][0][deathtype] +=1

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
                globalDamageReception[0][damageType] += damageDone                      # global local
                globalDamageReception[1] += damageDone                                  # global total
                speciesDamageReception[currSpecies][0][damageType] += damageDone     # local
                speciesDamageReception[currSpecies][1] += damageDone                 # total 

                # Diet damage received data
                if damageType == 'Starvation':
                    dietInfo[creatureDiet][1][0] += damageDone
                dietInfo[creatureDiet][1][1] += damageDone
            
            # Adulthood events

    return globalDeathCauses, speciesDeathCauses, globalDamageReception, speciesDamageReception, dietInfo

def ProcessResults(globalDeathCauses: dict, speciesDeathCauses: dict, globalDamageReception: dict, speciesDamageReception: dict, dietInfo: dict):
    # Open the analysis output file
    # outputFile = open('analysis.txt', 'w')
    # AppendFile_n_Print(f'Total Creatures : {totalCreatures}', outputFile)
    # ProcessDict(globalDeathCauses, totalCreatures, outputFile)
    #ShowGlobalInfo(globalDeathCauses[0], globalDamageReception[0])
    ShowDietInfo(dietInfo)
    damageType = 'Starvation'
    ShowSpeciesBarChart(f'Species {damageType} percentage', ['Species', 'Percentage of deaths'], speciesDamageReception, damageType) 
    # deathCause = 'Starved'
    # ShowSpeciesBarChart(f'Species {deathCause} percentage', ['Species', 'Percentage of damage'], speciesDeathCauses, deathCause)
    # DictionaryPieChart('Global death causes', globalDeathCauses)
    
    # for k,v in speciesDeathCauses.items():
    #     AppendFile_n_Print(f'\n{k} Total Creatures : {v[1]}', outputFile)
    #     ProcessDict(v[0], v[1], outputFile)
#endregion

def main():
    globalDeathCauses, speciesDeathCauses, globalDamageReception, speciesDamageReception, dietInfo = ProcessData()
    ProcessResults(globalDeathCauses, speciesDeathCauses, globalDamageReception, speciesDamageReception, dietInfo)  

main()