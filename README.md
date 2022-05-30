# Authors

Daniel González Cerdeiras
Georgi Mednikov
Eloy Moreno Cortijo
Daniel Cortijo Gamboa
Andrés de la Cuesta López
Pablo Rodríguez-Bobada García-Muñoz

# Summary

This is a study about the creation of a procedural generator of characters, more specifically animals, which can be imported and interpreted however one wants, since it generates data based results. Procedural generators are one of the most convenient mechanisms available when it comes to creating content in a computer program , as long as it is reasonable within its context. This is due to this procedure reducing human labour through the automatization of tasks. The concept of procedural generators references the set of techniques used mainly in the videogame industry, and are used to accomplish a variety of goals, from generating maps or virtual worlds to music with dynamic motifs or the aforementioned generation of characters.
An animal is the consequence of a process of adaptation to its environment, and therefore its properties represent to a certain degree the characteristics of the latter. This is a factor that can be analyzed and exploited. Given a terrain with a certain set of characteristics, this study pretends to create a series of creatures that interact among themselves and with the world they inhabit, simulating the concept of evolution to create a believable, realistic, sustainable environment. Furthermore, in order to prove the veracity of the simulation, it is exported to an external engine to demonstrate the inner workings of the simulation and the utility of the resulting data.
This project is closely related to artificial intelligence and based around for main branches of knowledge: procedural generation, in order to create an inhabited world; behavioral engineering, using a state machine and pathfinding to simulate the behaviors of the creatures; evolutionary computing, to represent the creatures’ genetic information and allow for realistic reproduction and, lastly, a certain amount of expertise related to the external engine chosen in which the resulting data will be displayed as an example.
In an effort to evaluate the correct performance of this program two methods will be used: a user based test in which people alien to the project evaluate the resulting data based on their experience with it and using a event telemetry system that allows for the collection of data for a posterior analysis, so that conclusions can be reached on how to improve the effectiveness of the project.

# Usage

There are two possible executions of the program: one through the console and the other through Windows prompts. They are different projects in the solutions and therefore different executables, but both have the same functionalities. When any of the two is run, the program will ask for valid information though the designated medium. If it is not valid, like an address not existing, it will ask again ultil provided.
The program has a large amount of costumization through values, which it has assigned by default. In order to personalize the simulation, certain files have to be provided before the execution of the program. The ones relating to the structure of the classes, such as the world configuration or the chromosome are given by default and therefore their values can be modified. If a concrete terrain, for example, is desired, it has to be provided in the folder designated has the input folder when the program asks for information. Specific information about the files that can be provided is pending.

The resulting data will be created in the folder designated when the program asked for information. It will consist of the information of the remaining species, the information of the world generated with flora and the phylogenetic tree of the species along the years.
