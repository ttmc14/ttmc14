import os

for filename in os.listdir("."):
    if "Brute" in filename:
        new_name = filename.replace("Brute", "MCBrute")
        os.rename(filename, new_name)
        print(f"{filename} -> {new_name}")
