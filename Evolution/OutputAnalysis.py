import glob
import json
import plotly.express as px
import plotly.graph_objects as pgo
from plotly.subplots import make_subplots

#region Data Processing

# The session data is processed returning the following structures in a dictionary with the next entries:
#   > 'SessionInfo': A dictionary with general information from the simulation (gathered from the SimulationStart event).
#       It contains the next entries: 'Type', 'Timestamp', 'SessionID', 'YearTicks', 'TotalTicks', 'TotalEdiblePlants'
#   > 'GlobalDeathCauses' A tuple where the first value is a dictionary where each key represents the 
#       death type and each value the number of creatures of that species that died, and the second value
#       is the total number of creatures that existed.
#   > 'SpeciesDeathCauses': A dictionary where keys are the name of a species and the values are a tuple where
#       the first value is a dictionary where each key represents the death type and each value the number of creatures of that species that died,
#       and the second value is the number of creatures of that species.
#   > 'GlobalDamageReception' A tuple where the first value is a dictionary where each key represents the 
#       damage type and each value the total damage done to all creatures with that type of damage, and the second value
#       is the total damage done to all creatures that existed.
#   > 'SpeciesDamageReception': A dictionary where keys are the name of a species and the values are a tuple where
#       the first value is a dictionary where each key represents the damage type and each value the total damage done to that species
#       with that type of damage, and the second value is the toal damage done to that species.
#       and the second value is the number of creatures of that species.
#   > 'GlobalBirthInfo': A array of 4 values, where each value has the nex information:
#           [0] -> Global Adulthood rate, the percentage of creatures that reach adulthood
#           [1] -> Global reproduction ratio, the average of adults that get to offspring during its lifetime
#           [2] -> The list of all births. A birth is a tuple where the first value is the number of childs and the second value is the tick when the birth happened
#           [3] -> The list of all deaths. A death is represented with the tick when the death happened
#   > 'SpeciesBirthInfo': A dictionary where keys are the name of a species and the values are an array of 4 values, where each value has the nex information:
#           [0] -> Species Adulthood rate, the percentage of creatures from that species that reach adulthood
#           [1] -> Species reproduction ratio, the average of adults from that species that get to offspring during its lifetime
#           [2] -> The list of all births from that species. A birth is a tuple where the first value is the number of childs and the second value is the tick when the birth happened
#           [3] -> The list of all deaths from that species. A death is represented with the tick when the death happened
#   > 'DietInfo': A dictionary with an entry per diet type ('Omnivores', 'Hervibores', 'Carnivores') and each entry has two values:
#       - The first value is tuple where the first value is the number of deaths due to starvation and the second value is the total number of deaths with the corresponding diet.
#       - The second value is tuple where the first value is the damage recived due to starvation and the second value is the total number of damage recieved with the corresponding diet.
#   > 'PlantsEaten': A list with all the events that are fired when a creature eats a plant.
#   > 'SpeciesDrinkSuccess': A dictionary where keys are the name of a species and values are the ratio of success that that species has gotten at drinking water
#  
# The visualization methods require this data (or some of this data, depending on the method) to function properly
#
def ProcessData(path: str):
    # Get all the species folder in the Output Dir, and the sessionOutput file
    speciesList = glob.glob(f'{path}/*/')
    sessionOutput = json.load(open(f'{path}\\\\sessionOutput.json'))

    # SimulationStart Event, we know it is the second event
    simulationStartEvent = sessionOutput[1]
    # Get plants eaten event
    plantsEatenEvents = [x for x in sessionOutput if (x['Type'] == 'PlantEaten')]
    
    totalCreatures = 0
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
    # [2] -> Births ticks + numChilds list
    # [3] -> Deaths ticks list
    globalBirthInfo = [0, 0, [], []]
    speciesBirthInfo = {}

    globalAdultCreatures = 0
    globalOriginalCreatures = 0
    globalMatingCreatures = 0

    # Drink 
    # { 'species': (0.9) ...}
    speciesDrinkSucces = {}

    # Get the death events from every creature in every species folder and add it to the pertinent dictionary
    for i in range(len(speciesList)):
        currSpecies = speciesList[i][len(path)+1:-1]   # remove dir\\
        aux = f'{speciesList[i]}*.json'
        creature_list = glob.glob(f'{speciesList[i]}*.json')
        creaturesNum = len(creature_list)
        speciesDeathCauses[currSpecies] = (dict.fromkeys(['Temperature','Attack','Retalliation','Starved','Dehydration','Exhaustion','Poisoned','Longevity','SimulationEnd'], 0), len(creature_list))
        totalCreatures += creaturesNum
        # [ 'species', (dict [damagetype, damageDone] , totalDamage) ]
        speciesDamageReception[currSpecies] = [dict.fromkeys(['Temperature','Attack','Retalliation','Starvation','Dehydration','Exhaustion','Poison'], 0), 0]
        speciesGoToDrinkEvents = 0
        speciesDrinkEvents = 0
        
        speciesBirthInfo[currSpecies] = [0, 0, [], []]
        originalCreatures = 0
        adultCreatures = 0
        matingCreatures = 0
        for j in range(creaturesNum):
            creatureData = json.load(open(creature_list[j]))
            # Birth data, to acces to creature stats
            birthData = [x for x in creatureData if (x['Type'] == 'CreatureBirth')]
            # Death data
            filteredData = [x for x in creatureData if (x['Type'] == 'CreatureDeath')]            
            creatureDiet = birthData[0]['Diet']
            deathEvent = filteredData[0]
            deathtype = filteredData[0]['DeathType']
            globalDeathCauses[0][deathtype] += 1
            speciesDeathCauses[currSpecies][0][deathtype] +=1

            # Diet death 
            if deathtype != 'SimulationEnd':
                dietInfo[creatureDiet][0][1] += 1
                if deathtype == 'Starved':
                    dietInfo[creatureDiet][0][0] += 1
                # Also register the death in the birth data
                speciesBirthInfo[currSpecies][3].append(deathEvent['Tick']) 
                globalBirthInfo[3].append(deathEvent['Tick']) 

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
            
            # Drinking events
            filteredData = [x for x in creatureData if (x['Type'] == 'CreatureStateEntry')]
            speciesGoToDrinkEvents += len([x for x in filteredData if (x['State'] == 'GoToDrinkState')])
            speciesDrinkEvents += len([x for x in filteredData if (x['State'] == 'DrinkingState')])

            # Birth events
            filteredData = [x for x in creatureData if (x['Type'] == 'CreatureAdult')]
            numAdultEvent = len(filteredData) # will always be 1 or 0
            adultCreatures += numAdultEvent
            if numAdultEvent > 0 and filteredData[0]['Original'] :
                originalCreatures +=1
            filteredData = [x for x in creatureData if (x['Type'] == 'CreatureMating')]
            matingCreatures += len(filteredData)

            for mEvent in filteredData:
                birth = [mEvent['ChildNumber'], mEvent['Tick']]
                speciesBirthInfo[currSpecies][2].append(birth)
                globalBirthInfo[2].append(birth)


        # Max 1 to avoid division by 0 which happens when there are no goToDrink events, so in that case it is 0/1=0
        speciesDrinkSucces[currSpecies] = speciesDrinkEvents / max(1,speciesGoToDrinkEvents) * 100

        # Update Birth info
        speciesBirthInfo[currSpecies][0] = adultCreatures - originalCreatures / max(1, creaturesNum - originalCreatures)
        # Max 1 to avoid division by 0 which happens when there are no goToDrink events, so in that case it is 0/1=0
        speciesBirthInfo[currSpecies][1] = matingCreatures / max(1,adultCreatures)
        
        globalAdultCreatures += adultCreatures
        globalOriginalCreatures += originalCreatures
        globalMatingCreatures += matingCreatures

    globalDeathCauses[1] = totalCreatures
    globalBirthInfo[0] = globalAdultCreatures - globalOriginalCreatures / max(1,totalCreatures - globalOriginalCreatures)
    globalBirthInfo[1] = globalMatingCreatures / globalAdultCreatures

    returnDict = {
        'SessionInfo': simulationStartEvent,
        'GlobalDeathCauses' : globalDeathCauses,
        'SpeciesDeathCauses' : speciesDeathCauses,
        'GlobalDamageReception' : globalDamageReception,
        'SpeciesDamageReception' : speciesDamageReception,
        'GlobalBirthInfo' : globalBirthInfo,
        'SpeciesBirthInfo': speciesBirthInfo,
        'DietInfo' : dietInfo,
        'PlantsEaten': plantsEatenEvents,
        'SpeciesDrinkSuccess': speciesDrinkSucces
    }
    return returnDict
#endregion

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

def ShowSpeciesCausesChart(title, axisNames, speciesDict: dict, cause:str):
    speciesNames = list(speciesDict.keys())
    speciesDeathCausePerc = list()
    for species in speciesNames:
        perc = speciesDict[species][0][cause] / max(1,speciesDict[species][1] * 100)
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

def ShowPlantsConsumedInfo(yearTicks, totalTicks, plantsEaten, totalPlants):
    totalYears = int(totalTicks / yearTicks)
    yearlyConsumption = [0]*totalYears
    for i in range(len(plantsEaten)):
        yearlyConsumption[int(plantsEaten[i]['Tick'] / yearTicks)] += 1
    yearlyConsumption =  [x / totalPlants * 100 for x in yearlyConsumption]

    LineChart('Percentage of plants consumed through the years', ['Years', 'Percentage of plants consumed'], list(range(totalYears)), yearlyConsumption)

# Shows the birth and death line graphs throughout all the simulation years
def ShowBirthsAndDeaths(name, yearTicks, totalTicks, birthList: list, deathList: list):
    years = int(totalTicks / yearTicks)
    births = [0]*years
    deaths = [0]*years
    for i in range(len(birthList)):
        births[int(birthList[i][1] / yearTicks)] += birthList[i][0]
    for i in range(len(deathList)):
        deaths[int(deathList[i] / yearTicks)] += 1

    fig = make_subplots(rows=1, cols=2, shared_yaxes=True, subplot_titles=(
        f'{name} births through the years',
        f'{name} deaths through the years'))

    fig.add_trace(pgo.Scatter(x=list(range(1, years + 1)), y=births), 1, 1)
    fig.add_trace(pgo.Scatter(x=list(range(1, years + 1)), y=deaths), 1, 2)
    fig.update_xaxes(title='Years')
    fig.update_yaxes(title='Number of creatures')
    fig.show()

# Shows the natality data depending on the provided index
def ShowAdulthood(globalBirthInfo:list , speciesBirthInfo: dict):
    aux = [x[0] * 100 for x in list(speciesBirthInfo.values())]
    BarChart(f'Percentage of creatures that reach adulthood. Global: {globalBirthInfo[0] *100}', ['Species', 'Percentage'], list(speciesBirthInfo.keys()), aux)

def ShowOffspring(globalBirthInfo:list , speciesBirthInfo: dict):
    aux = [x[1] for x in list(speciesBirthInfo.values())]
    BarChart(f'Average offspring per adult. Global: {globalBirthInfo[1]}', ['Species', 'Percentage'], list(speciesBirthInfo.keys()), aux)
#endregion