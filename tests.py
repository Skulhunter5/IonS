import sys
import subprocess
from os import listdir
from os.path import isfile, join

def diff(a, b):
    print("Expectation:")
    print(a)
    print(">>>")
    print("Actual output:")
    print(b)
    print(">>>")
    m = len(a) if len(a) >= len(b) else len(b)
    column = 0
    line = 0
    i = 0
    while(i < m):
        if(a[i] == "\n"):
            column = 0
            line += 1
        if(line == 0):
            pass
        elif(i >= len(a)):
            print("  Difference: the expectation is shorter than the actual output")
            return
        elif(i >= len(b)):
            print("  Difference: the expectation is longer than the actual output")
            return
        elif(a[i] != b[i]):
            print("    " + str(line) + ":" + str(column) + ":\n      Expected: '" + a[i] + "'\n      Got: '" + b[i] + "'")
            return
        column += 1
        i += 1
    print("Internal error: No difference")

def run(runFor=None):
    if(runFor != None):
        files = [f for f in listdir("tests") if isfile(join("tests", f)) and f.endswith(".ions")]
        for file in files:
            if(file != runFor):
                continue
            print("'" + file + "':")
            if(not isfile("tests/" + file[:-5] + ".txt")):
                print("  No expectation found\n")
                return
            with open("tests/" + file[:-5] + ".txt", 'r') as f:
                lines = f.readlines()
                transcriptionProcess = subprocess.run(["dotnet", "run", "--compile", "tests/" + file], stdout=subprocess.PIPE)
                if(transcriptionProcess.returncode != 0):
                    transcriptionOutput = transcriptionProcess.stdout.decode('utf-8').replace("\r\n", '\n')
                    if(not lines[0][4:].startswith("Transcription")):
                        print("  Failed: Error during transcription (exitcode=" + str(transcriptionProcess.returncode) + "):\n")
                        print(transcriptionOutput)
                        return
                    if(transcriptionProcess.returncode != int(lines[0][27:-1])):
                        print("  Failed: Transcription exited with code " + str(transcriptionProcess.returncode) + " instead of " + str(lines[0][27:-1]))
                        print(transcriptionOutput)
                        return
                    expectation = ""
                    for line in lines[1:]:
                        expectation += line
                    if(expectation == transcriptionOutput):
                        print("  Passed")
                        return
                    diff(expectation, transcriptionOutput)
                    return
                compilationProcess = subprocess.run(["wsl", "--exec", "/shared/compIonsTest"], stdout=subprocess.PIPE)
                if(compilationProcess.returncode != 0):
                    compilationOutput = compilationProcess.stdout.decode('utf-8').replace("\r\n", '\n')
                    if(not lines[0][4:].startswith("Compilation")):
                        print("  Failed: Error during compilation (exitcode=" + str(compilationProcess.returncode) + "):\n")
                        print(compilationOutput)
                        return
                    if(compilationProcess.returncode != int(lines[0][25:-1])):
                        print("  Failed: Compilation exited with code " + str(compilationProcess.returncode) + " instead of " + str(lines[0][27:-1]))
                        print(compilationOutput)
                        return
                    expectation = ""
                    for line in lines[1:]:
                        expectation += line
                    if(expectation == compilationOutput):
                        print("  Passed")
                        return
                    diff(expectation, compilationOutput)
                    return
                executionProcess = subprocess.run(["wsl", "--exec", "/shared/testIons"], stdout=subprocess.PIPE)
                if(executionProcess.returncode != int(lines[0][23:-1])):
                    print("  Failed: Execution finished with incorrect exitcode")
                    return
                executionOutput = executionProcess.stdout.decode('utf-8').replace("\r\n", '\n')
                expectation = ""
                for line in lines[1:]:
                    expectation += line
                if(executionOutput == expectation):
                    print("  Passed")
                    return
                else:
                    print("  Failed:\n")
                    diff(expectation, executionOutput)
                    return
    skippedCounter = 0
    passedCounter = 0
    failedCounter = 0
    failedTests = []
    files = [f for f in listdir("tests") if isfile(join("tests", f)) and f.endswith(".ions")]
    for file in files:
        if(file.endswith(".ions")):
            print("'" + file + "':")
            if(not isfile("tests/" + file[:-5] + ".txt")):
                print("  No expectation found\n")
                skippedCounter += 1
                continue
            with open("tests/" + file[:-5] + ".txt", 'r') as f:
                lines = f.readlines()
                transcriptionProcess = subprocess.run(["dotnet", "run", "--compile", "tests/" + file], stdout=subprocess.PIPE)
                if(transcriptionProcess.returncode != 0):
                    transcriptionOutput = transcriptionProcess.stdout.decode('utf-8').replace("\r\n", '\n')
                    if(not lines[0][4:].startswith("Transcription")):
                        print("  Failed: Error during transcription (exitcode=" + str(transcriptionProcess.returncode) + "):\n")
                        print(transcriptionOutput)
                        failedCounter += 1
                        failedTests.append(file)
                        continue
                    if(transcriptionProcess.returncode != int(lines[0][27:-1])):
                        print("  Failed: Transcription exited with code " + str(transcriptionProcess.returncode) + " instead of " + str(lines[0][27:-1]))
                        print(transcriptionOutput)
                        failedCounter += 1
                        failedTests.append(file)
                        continue
                    expectation = ""
                    for line in lines[1:]:
                        expectation += line
                    if(expectation == transcriptionOutput):
                        print("  Passed\n")
                        passedCounter += 1
                        continue
                    diff(expectation, transcriptionOutput)
                    failedCounter += 1
                    failedTests.append(file)
                    continue
                compilationProcess = subprocess.run(["wsl", "--exec", "/shared/compIonsTest"], stdout=subprocess.PIPE)
                if(compilationProcess.returncode != 0):
                    compilationOutput = compilationProcess.stdout.decode('utf-8').replace("\r\n", '\n')
                    if(not lines[0][4:].startswith("Compilation")):
                        print("  Failed: Error during compilation (exitcode=" + str(compilationProcess.returncode) + "):\n")
                        print(compilationOutput)
                        failedCounter += 1
                        failedTests.append(file)
                        continue
                    if(compilationProcess.returncode != int(lines[0][25:-1])):
                        print("  Failed: Compilation exited with code " + str(compilationProcess.returncode) + " instead of " + str(lines[0][27:-1]))
                        print(compilationOutput)
                        failedCounter += 1
                        failedTests.append(file)
                        continue
                    expectation = ""
                    for line in lines[1:]:
                        expectation += line
                    if(expectation == compilationOutput):
                        print("  Passed\n")
                        passedCounter += 1
                        continue
                    diff(expectation, compilationOutput)
                    failedCounter += 1
                    failedTests.append(file)
                    continue
                executionProcess = subprocess.run(["wsl", "--exec", "/shared/testIons"], stdout=subprocess.PIPE)
                if(not lines[0][4:].startswith("Execution")):
                    print("  Failed: Should have failed before execution")
                    print("  Transcription:\n")
                    print(transcriptionProcess.stdout.decode('utf-8').replace("\r\n", '\n'))
                    print("  Compilation:\n")
                    print(compilationProcess.stdout.decode('utf-8').replace("\r\n", '\n'))
                    failedCounter += 1
                    failedTests.append(file)
                    continue
                if(executionProcess.returncode != int(lines[0][23:-1])):
                    print("  Failed: Execution finished with incorrect exitcode")
                    failedCounter += 1
                    failedTests.append(file)
                    continue
                executionOutput = executionProcess.stdout.decode('utf-8').replace("\r\n", '\n')
                expectation = ""
                for line in lines[1:]:
                    expectation += line
                if(executionOutput == expectation):
                    print("  Passed\n")
                    passedCounter += 1
                    continue
                else:
                    print("  Failed:\n")
                    diff(expectation, executionOutput)
                    failedCounter += 1
                    failedTests.append(file)
                    continue
    print("Result:\n  Passed: " + str(passedCounter) + "\n  Skipped: " + str(skippedCounter) + "\n  Failed: " + str(failedCounter))
    for test in failedTests:
        print("  - " + test)

def generate(forceGenerate = False, generateFor = None):
    if(generateFor != None):
        files = [f for f in listdir("tests") if isfile(join("tests", f)) and f.endswith(".ions")]
        for file in files:
            if(file != generateFor):
                continue
            print("'" + file + "':")
            with open("tests/" + file[:-5] + ".txt", 'w') as out:
                transcriptionProcess = subprocess.run(["dotnet", "run", "--compile", "tests/" + file], stdout=subprocess.PIPE)
                if(transcriptionProcess.returncode != 0):
                    out.write("--->Transcription:exitcode=" + str(transcriptionProcess.returncode) + "\n")
                    out.write(transcriptionProcess.stdout.decode('utf-8').replace("\r\n", '\n'))
                    print("  Generated expectation")
                    return
                compilationProcess = subprocess.run(["wsl", "--exec", "/shared/compIonsTest"], stdout=subprocess.PIPE)
                if(compilationProcess.returncode != 0):
                    out.write("--->Compilation:exitcode=" + str(compilationProcess.returncode) + "\n")
                    out.write(compilationProcess.stdout.decode('utf-8').replace("\r\n", '\n'))
                    print("  Generated expectation")
                    return
                executionProcess = subprocess.run(["wsl", "--exec", "/shared/testIons"], stdout=subprocess.PIPE)
                out.write("--->Execution:exitcode=" + str(executionProcess.returncode) + "\n")
                out.write(executionProcess.stdout.decode('utf-8').replace("\r\n", '\n'))
                print("  Generated expectation")
            return
    generatedCounter = 0
    keptCounter = 0
    files = [f for f in listdir("tests") if isfile(join("tests", f)) and f.endswith(".ions")]
    for file in files:
        if(file.endswith(".ions")):
            print("'" + file + "':")
            if(isfile("tests/" + file[:-5] + ".txt") and not forceGenerate):
                print("  Expectation found\n")
                keptCounter += 1
                continue
            with open("tests/" + file[:-5] + ".txt", 'w') as out:
                transcriptionProcess = subprocess.run(["dotnet", "run", "--compile", "tests/" + file], stdout=subprocess.PIPE)
                if(transcriptionProcess.returncode != 0):
                    out.write("--->Transcription:exitcode=" + str(transcriptionProcess.returncode) + "\n")
                    out.write(transcriptionProcess.stdout.decode('utf-8'))
                    print("  Generated expectation\n")
                    generatedCounter += 1
                    continue
                compilationProcess = subprocess.run(["wsl", "--exec", "/shared/compIonsTest"], stdout=subprocess.PIPE)
                if(compilationProcess.returncode != 0):
                    out.write("--->Compilation:exitcode=" + str(compilationProcess.returncode) + "\n")
                    out.write(compilationProcess.stdout.decode('utf-8'))
                    print("  Generated expectation\n")
                    generatedCounter += 1
                    continue
                executionProcess = subprocess.run(["wsl", "--exec", "/shared/testIons"], stdout=subprocess.PIPE)
                out.write("--->Execution:exitcode=" + str(executionProcess.returncode) + "\n")
                out.write(executionProcess.stdout.decode('utf-8'))
                print("  Generated expectation\n")
                generatedCounter += 1
    print("Result:\n  Generated: " + str(generatedCounter) + "\n  Kept: " + str(keptCounter))

if(__name__ == "__main__"):
    files = [f for f in listdir("tests") if isfile(join("tests", f)) and f.endswith(".ions")]
    if(len(sys.argv) == 1 or sys.argv[1] in ["-r", "--run"]):
        if(len(sys.argv) >= 4 and sys.argv[2] in ["-t", "--test"]):
            run(runFor=sys.argv[3])
        else:
            run()
    elif(sys.argv[1] in ["-g", "--generate"]):
        if(len(sys.argv) >= 3 and sys.argv[2] in ["-f", "--force"]):
            generate(forceGenerate=True)
        elif(len(sys.argv) >= 4 and sys.argv[2] in ["-t", "--test"]):
            generate(generateFor=sys.argv[3])
        else:
            generate()
