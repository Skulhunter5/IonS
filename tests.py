import sys
import subprocess
from os import listdir
from os.path import isfile, join

executablePath = "bin\\Debug\\net5.0\\IonS.dll"

def diff(a, b):
    print("Expectation:")
    print(a)
    print(">>>")
    print("Actual output:")
    print(b)
    print(">>>")
    print(list(a))
    print(list(b))
    m = len(a) if len(a) >= len(b) else len(b)
    column = 0
    line = 0
    i = 0
    while(i < m):
        if(line == 0):
            pass
        if(i >= len(a)):
            print("  Difference: the expectation is shorter than the actual output\n")
            return
        elif(i >= len(b)):
            print("  Difference: the expectation is longer than the actual output\n")
            return
        if(a[i] == "\n"):
            column = 0
            line += 1
        if(a[i] != b[i]):
            print("    " + str(line) + ":" + str(column) + ":\n      Expected: '" + a[i] + "'\n      Got: '" + b[i] + "'\n")
            return
        column += 1
        i += 1
    print("Internal error: No difference\n")

RESULT_PASSED = 0
RESULT_FAILED = 1
RESULT_SKIPPED = 2
RESULT_GENERATED = 3

def runFile(file):
    print("'" + file + "':")
    with open("tests/" + file[:-5] + ".txt", 'r') as f:
        lines = f.readlines()
        transcriptionProcess = subprocess.run(["dotnet", executablePath, "--compile", "tests/" + file], stdout=subprocess.PIPE, stderr=subprocess.PIPE)
        if(transcriptionProcess.returncode != 0):
            transcriptionOutput = transcriptionProcess.stdout.decode('utf-8').replace("\r\n", '\n')
            if(not lines[0][4:].startswith("Transcription")):
                print("  Failed: Error during transcription (exitcode=" + str(transcriptionProcess.returncode) + "):\n")
                print(transcriptionOutput)
                return RESULT_FAILED
            if(transcriptionProcess.returncode != int(lines[0][27:-1])):
                print("  Failed: Transcription exited with code " + str(transcriptionProcess.returncode) + " instead of " + str(lines[0][27:-1]))
                print(transcriptionOutput)
                return RESULT_FAILED
            expectation = ""
            for line in lines[1:]:
                expectation += line
            if(expectation == transcriptionOutput):
                print("  Passed\n")
                return RESULT_PASSED
            print("  Failed:\n")
            diff(expectation, transcriptionOutput)
            return RESULT_FAILED
        compilationProcess = subprocess.run(["wsl", "--exec", "/shared/compIonsTest"], stdout=subprocess.PIPE, stderr=subprocess.PIPE)
        if(compilationProcess.returncode != 0):
            compilationOutput = compilationProcess.stdout.decode('utf-8').replace("\r\n", '\n')
            if(not lines[0][4:].startswith("Compilation")):
                print("  Failed: Error during compilation (exitcode=" + str(compilationProcess.returncode) + "):\n")
                print(compilationOutput)
                return RESULT_FAILED
            if(compilationProcess.returncode != int(lines[0][25:-1])):
                print("  Failed: Compilation exited with code " + str(compilationProcess.returncode) + " instead of " + str(lines[0][27:-1]))
                print(compilationOutput)
                return RESULT_FAILED
            expectation = ""
            for line in lines[1:]:
                expectation += line
            if(expectation == compilationOutput):
                print("  Passed\n")
                return RESULT_PASSED
            print("  Failed:\n")
            diff(expectation, compilationOutput)
            return RESULT_FAILED
        executionProcess = subprocess.run(["wsl", "--exec", "/shared/testIons"], stdout=subprocess.PIPE, stderr=subprocess.PIPE)
        if(executionProcess.returncode != int(lines[0][23:-1])):
            print("  Failed: Execution finished with incorrect exitcode")
            return RESULT_FAILED
        executionOutput = executionProcess.stdout.decode('utf-8').replace("\r\n", '\n')
        expectation = ""
        for line in lines[1:]:
            expectation += line
        if(executionOutput == expectation):
            print("  Passed\n")
            return RESULT_PASSED
        else:
            print("  Failed:\n")
            diff(expectation, executionOutput)
            return RESULT_FAILED

def run(files):
    skippedCounter = 0
    passedCounter = 0
    failedCounter = 0
    failedTests = []
    for file in files:
        if(file.endswith(".ions")):
            if(not isfile("tests/" + file[:-5] + ".txt")):
                print("  No expectation found\n")
                skippedCounter += 1
                continue
            result = runFile(file)
            if(result == RESULT_PASSED):
                passedCounter += 1
            elif(result == RESULT_FAILED):
                failedCounter += 1
    print("Result:\n  Passed: " + str(passedCounter) + "\n  Skipped: " + str(skippedCounter) + "\n  Failed: " + str(failedCounter))
    for test in failedTests:
        print("  - " + test)

def generateFile(file):
    with open("tests/" + file[:-5] + ".txt", 'w') as out:
        transcriptionProcess = subprocess.run(["dotnet", executablePath, "--compile", "tests/" + file], stdout=subprocess.PIPE, stderr=subprocess.PIPE)
        if(transcriptionProcess.returncode != 0):
            out.write("--->Transcription:exitcode=" + str(transcriptionProcess.returncode) + "\n")
            out.write(transcriptionProcess.stdout.decode('utf-8').replace("\r\n", '\n'))
            print("  Generated expectation\n")
            return RESULT_GENERATED
        compilationProcess = subprocess.run(["wsl", "--exec", "/shared/compIonsTest"], stdout=subprocess.PIPE, stderr=subprocess.PIPE)
        if(compilationProcess.returncode != 0):
            out.write("--->Compilation:exitcode=" + str(compilationProcess.returncode) + "\n")
            out.write(compilationProcess.stdout.decode('utf-8').replace("\r\n", '\n'))
            print("  Generated expectation\n")
            return RESULT_GENERATED
        executionProcess = subprocess.run(["wsl", "--exec", "/shared/testIons"], stdout=subprocess.PIPE, stderr=subprocess.PIPE)
        out.write("--->Execution:exitcode=" + str(executionProcess.returncode) + "\n")
        out.write(executionProcess.stdout.decode('utf-8').replace("\r\n", '\n'))
        print("  Generated expectation\n")
        return RESULT_GENERATED

def generate(files, forceGenerate=False):
    generatedCounter = 0
    keptCounter = 0
    for file in files:
        if(file.endswith(".ions")):
            print("'" + file + "':")
            if(isfile("tests/" + file[:-5] + ".txt") and not forceGenerate):
                print("  Expectation found\n")
                keptCounter += 1
                continue
            result = generateFile(file)
            if(result == RESULT_GENERATED):
                generatedCounter += 1
    print("Result:\n  Generated: " + str(generatedCounter) + "\n  Kept: " + str(keptCounter))

if(__name__ == "__main__"):
    args = sys.argv.copy()
    i = 1
    # parameters
    action = None
    force = False
    filenames = None
    build = True
    # parsing the arguments
    while(i < len(args)):
        # action
        if(i == 1):
            if(args[i] in ["r", "run"]):
                action = "run"
            elif(args[i] in ["g", "generate"]):
                action = "generate"
            else:
                print("Invalid action: '" + args[i] + "'")
                exit()
            i += 1
            continue
        # general arguments
        if(args[i] == "-t"):
            i += 1
            if(i >= len(args)):
                print("Missing argument for -t | --test")
                exit()
            filenames = args[i].split(',')
            i += 1
            continue
        if(args[i] in ["-nb", "--no-build"]):
            build = False
            i += 1
            continue
        # action-specific arguments
        if(action == "generate"):
            if(args[i] in ["-f", "--force"]):
                force = True
                i += 1
                continue
            else:
                print("Invalid argument: '" + args[i] + "'")
                exit()
        print("Invalid argument: '" + args[i] + "'")
        exit()
    files = [f for f in listdir("tests") if isfile(join("tests", f)) and f.endswith(".ions")]
    if(filenames != None):
        files = list(filter(lambda filename: filename in filenames, files))
    if(build):
        subprocess.run(["dotnet", "build"], stdout=subprocess.PIPE, stderr=subprocess.PIPE)
    if(action == "run"):
        run(files)
    elif(action == "generate"):
        generate(files, forceGenerate=force if filenames == None else True)
    else:
        run()
