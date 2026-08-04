from pathlib import Path
from PIL import Image

COLOR_MAP = {
    "#769076": "#ccd3d8",
    "#6a806a": "#b7c3c6",
    "#576857": "#949da4",
    "#3e453c": "#808b90",
    "#282c27": "#6a6a7b",
}

def hex_to_rgb(hex_color):
    hex_color = hex_color.lstrip("#")
    return tuple(int(hex_color[i:i+2], 16) for i in (0, 2, 4))

color_map = {
    hex_to_rgb(src): hex_to_rgb(dst)
    for src, dst in COLOR_MAP.items()
}

for file in Path(__file__).parent.glob("*.png"):
    img = Image.open(file).convert("RGBA")
    pixels = img.load()

    width, height = img.size

    for y in range(height):
        for x in range(width):
            r, g, b, a = pixels[x, y]
            if (r, g, b) in color_map:
                nr, ng, nb = color_map[(r, g, b)]
                pixels[x, y] = (nr, ng, nb, a)

    img.save(file)
