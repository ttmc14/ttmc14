from pathlib import Path
from PIL import Image
import json
import shutil


def to_pascal_case(name: str) -> str:
    parts = name.replace("-", "_").split("_")
    return "".join(part.capitalize() for part in parts if part)


META = {
    "version": 1,
    "license": "CC-BY-NC-SA",
    "copyright": "https://github.com/tgstation/TerraGov-Marine-Corps",
    "size": {
        "x": 32,
        "y": 32
    },
    "states": [
        {
            "name": "equipped",
            "directions": 4
        },
        {
            "name": "icon"
        }
    ]
}

current_dir = Path.cwd()
parent_dir = current_dir.parent

for png_file in current_dir.glob("*.png"):
    stem = png_file.stem

    # имя папки в PascalCase
    folder_name = to_pascal_case(stem)

    rsi_dir = parent_dir / folder_name / "_template.rsi"
    rsi_dir.mkdir(parents=True, exist_ok=True)

    # equipped.png
    equipped_path = rsi_dir / "equipped.png"
    shutil.copy2(png_file, equipped_path)

    # icon.png
    with Image.open(png_file) as img:
        if img.size != (64, 64):
            print(f"Пропущен {png_file.name}: размер {img.size}, ожидался 64x64")
            continue

        # область x=32..63, y=0..31
        icon = img.crop((32, 0, 64, 32))
        icon.save(rsi_dir / "icon.png")

    # meta.json
    with open(rsi_dir / "meta.json", "w", encoding="utf-8") as f:
        json.dump(META, f, ensure_ascii=False, indent=2)

    print(f"Готово: {folder_name}")

print("Обработка завершена.")
