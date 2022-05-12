test_array = [0,1,2,3,4,5,6,7]

def getAllPosCombs(array):
    combs = []
    combs = getAllRotations(combs,array)
    return(combs)

def getAllRotations(combs,array,bolt=[]):
    for i in array:
        combs.append(bolt+array)
        array = rotateArray(array)
        if(len(array) > 1):
            getAllRotations(combs,array[1:len(array)],bolt+[array[0]])
    return(combs)

def rotateArray(array):
    rotated_array = array[1:len(array)] + array[0:1]
    return(rotated_array)

def saveListToFile(path,data):
    with open(path, "w+") as f:
        for i in data:
            f.write(str(i)+"\n")

if(__name__=="__main__"):
    combs = getAllPosCombs(test_array)
    print(len(combs))
    saveListToFile("list.txt",combs)
    
    combs_text = []
    for i in combs:
        temp = ""
        for j in i:
            temp += str(j)
        combs_text.append(temp)
    saveListToFile("text.txt",combs_text)

    combs_text_unique = set(combs_text)
    print(len(combs_text_unique))
    saveListToFile("text_unique.txt",combs_text_unique)
    
