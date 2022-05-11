import glob
from io import TextIOWrapper
import json
import plotly.express as px

def PieChart():
    fig = px.pie(names=['Dani', 'Georgi'], values=[89, 11], title='Contribución en Aprendizaje Automático')
    fig.show()

def BarChart():
    fig = px.bar(x=['Dani', 'Georgi'], y=[89, 11], title='Contribución en Aprendizaje Automático')
    fig.show()

def LineChart():
    fig = px.line(x=['Dani', 'Georgi'], y=[89, 11], title='Contribución en Aprendizaje Automático')
    fig.show()

def AppendFile_n_Print(data: str, file: TextIOWrapper):
    file.write(f'{data}\n')
    print(data)

def ProcessResults(results: dict, numCreatures: float, file: TextIOWrapper):
    deathCauses = {k: results[k] for k in results.keys() - {'SimulationEnd'}}
    aliveCreatures = {k: results[k] for k in results.keys() - deathCauses.keys()}
    AppendFile_n_Print(f'Last Alive : {aliveCreatures["SimulationEnd"]}\nDeathCauses:', file)
    for cause,num in deathCauses.items():
        AppendFile_n_Print(f'\t{cause} : {num / numCreatures * 100}', file)

def main():
    speciesList = glob.glob("*/")
    totalCreatures = 0
    globalDeathCauses = dict.fromkeys(['Temperature','Attack','Retalliation','Starved','Dehydration','Exhaustion','Poisoned','Longevity','SimulationEnd'], 0)
    speciesDeathCauses = {}
    outputFile = open('analysis.txt', 'w')
    for i in range(len(speciesList)):
        creature_list = glob.glob(f'{speciesList[i]}*.json')
        speciesDeathCauses[speciesList[i]] = (dict.fromkeys(['Temperature','Attack','Retalliation','Starved','Dehydration','Exhaustion','Poisoned','Longevity','SimulationEnd'], 0), len(creature_list))
        totalCreatures += len(creature_list)
        #print(f'{speciesList[i]} Deaths: \n')
        for j in range(len(creature_list)):
            creatureData = json.load(open(creature_list[j]))
            filteredData = [x for x in creatureData if (x['Type'] == 'CreatureDeath')]
            # filteredData[0] porque solo hay un evento de muerte
            id = filteredData[0]['CreatureID']
            deathtype = filteredData[0]['DeathType']
            globalDeathCauses[deathtype] += 1
            speciesDeathCauses[speciesList[i]][0][deathtype] +=1
            #print(f'{id}: DeathType: {deathtype}')

    AppendFile_n_Print(f'Total Creatures : {totalCreatures}', outputFile)
    ProcessResults(globalDeathCauses, totalCreatures, outputFile)
    for k,v in speciesDeathCauses.items():
        AppendFile_n_Print(f'\n{k} Total Creatures : {v[1]}', outputFile)
        ProcessResults(v[0], v[1], outputFile)

LineChart()