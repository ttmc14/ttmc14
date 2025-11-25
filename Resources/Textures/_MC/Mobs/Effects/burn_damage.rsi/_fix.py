import os

for filename in os.listdir("."):
    if "Burn" in filename:
        new_name = filename.replace("Burn", "MCBurn")
        os.rename(filename, new_name)
        print(f"{filename} -> {new_name}")
