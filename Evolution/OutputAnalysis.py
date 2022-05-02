import glob
import json

speciesList = glob.glob("*/")
for i in range(len(speciesList)):
    creature_list = glob.glob(f'{speciesList[i]}*.json')
    print(f'{speciesList[i]} Deaths: \n')
    for j in range(len(creature_list)):
        creatureData = json.load(open(creature_list[i]))
        filteredData = [x for x in creatureData if (x['Type'] == 'CreatureDeath')]
        # filteredData[0] porque solo hay un evento de muerte
        id = filteredData[0]['CreatureID']
        deathtype = filteredData[0]['DeathType']
        print(f'{id}: DeathType: {deathtype}')
