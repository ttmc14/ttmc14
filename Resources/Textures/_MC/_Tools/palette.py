import os
import re
import shutil
import tkinter as tk
from tkinter import filedialog, ttk, messagebox

import numpy as np
import yaml
from PIL import Image, ImageTk


# =========================
# Palette Loading
# =========================

def load_palettes_from_yaml(path: str = "palettes.yml") -> dict:
    if not os.path.exists(path):
        return {}

    try:
        with open(path, "r", encoding="utf-8") as f:
            return yaml.safe_load(f) or {}
    except Exception as e:
        print(f"YAML load error: {e}")
        return {}


def parse_palette(raw: str):
    hex_colors = re.findall(r"#([0-9a-fA-F]{6})", raw)
    return [
        (int(h[0:2], 16), int(h[2:4], 16), int(h[4:6], 16), 255)
        for h in hex_colors
    ]


# =========================
# Image Processing
# =========================

def get_template_colors(img: Image.Image):
    pixels = np.array(img.convert("RGBA"))
    alpha_mask = pixels[:, :, 3] > 0
    opaque_pixels = pixels[alpha_mask][:, :3]

    if len(opaque_pixels) == 0:
        return []

    unique_colors, counts = np.unique(opaque_pixels, axis=0, return_counts=True)
    top_colors = unique_colors[np.argsort(-counts)[:5]]

    brightness = [
        c[0] * 0.299 + c[1] * 0.587 + c[2] * 0.114
        for c in top_colors
    ]

    sorted_colors = top_colors[np.argsort(brightness)]
    return [tuple(c) for c in sorted_colors]


def apply_smart_palette(image: Image.Image, target_palette):
    template_colors = get_template_colors(image)
    if not template_colors:
        return image

    data = np.array(image.convert("RGBA"))
    new_data = np.zeros_like(data)

    r, g, b, a = (
        data[:, :, 0],
        data[:, :, 1],
        data[:, :, 2],
        data[:, :, 3],
    )

    for i, template_color in enumerate(template_colors):
        if i >= len(target_palette):
            break

        t_r, t_g, t_b = template_color
        p_r, p_g, p_b, _ = target_palette[i]

        mask = (r == t_r) & (g == t_g) & (b == t_b)
        new_data[mask] = [p_r, p_g, p_b, 255]

    new_data[:, :, 3] = a
    leftover = (new_data[:, :, 3] == 0) & (a > 0)
    new_data[leftover] = data[leftover]

    return Image.fromarray(new_data, "RGBA")


# =========================
# UI
# =========================

class ArmorApp:

    def __init__(self, root: tk.Tk):
        self.root = root
        self.root.title("APP Gay (TornadoTech) edition")
        self.root.geometry("1480x720")
        self.root.configure(bg="#1a1a1a")

        self.image = None
        self.processed = None
        self.scale = tk.IntVar(value=4)
        self.template_name = tk.StringVar(value="_template.rsi")
        self.skip_files = tk.StringVar()

        self.flat_palettes = self._load_palettes()
        self._build_ui()

    def _load_palettes(self):
        raw = load_palettes_from_yaml()
        flat = {}
        for category, items in raw.items():
            if not items:
                continue
            for name, hex_str in items.items():
                flat[f"[{category}] {name}"] = (name, parse_palette(hex_str))
        return flat

    def log(self, message: str):
        self.log_box.insert(tk.END, f"> {message}\n")
        self.log_box.see(tk.END)

    def _build_ui(self):
        header = tk.Frame(self.root, bg="#252525", pady=10)
        header.pack(fill=tk.X)

        tk.Button(header, text="Open Image", command=self.load_file,
                  bg="#444", fg="white", relief="flat").pack(side=tk.LEFT, padx=10)
        tk.Button(header, text="Process Folder", command=self.process_folder,
                  bg="#444", fg="white", relief="flat").pack(side=tk.LEFT, padx=5)
        tk.Button(header, text="Mass Process (All)", command=self.mass_process,
                  bg="#5c3c5c", fg="white", relief="flat").pack(side=tk.LEFT, padx=5)

        tk.Label(header, text="Template Folder:", bg="#252525",
                 fg="white").pack(side=tk.LEFT, padx=(15, 5))
        tk.Entry(header, textvariable=self.template_name, width=18,
                 bg="#333", fg="white", insertbackground="white").pack(side=tk.LEFT, padx=5)

        tk.Button(header, text="Mass Process By Template", command=self.mass_process_by_template,
                  bg="#3c4f6b", fg="white", relief="flat").pack(side=tk.LEFT, padx=5)

        tk.Label(header, text="Skip PNGs (comma):", bg="#252525",
                 fg="white").pack(side=tk.LEFT, padx=(10, 0))
        tk.Entry(header, textvariable=self.skip_files, width=20,
                 bg="#333", fg="white", insertbackground="white").pack(side=tk.LEFT, padx=5)

        pal_list = list(self.flat_palettes.keys())
        self.pal_select = ttk.Combobox(header, values=pal_list,
                                       width=35, state="readonly")
        self.pal_select.pack(side=tk.LEFT, padx=10)
        if pal_list:
            self.pal_select.current(0)
        self.pal_select.bind("<<ComboboxSelected>>", lambda _: self.update_preview())

        tk.Label(header, text="Zoom:", bg="#252525", fg="white").pack(side=tk.LEFT, padx=(10, 0))
        tk.Spinbox(header, from_=1, to=32, textvariable=self.scale, width=3,
                   command=self.render, bg="#333", fg="white").pack(side=tk.LEFT, padx=5)

        tk.Button(header, text="Save PNG", command=self.save_single,
                  bg="#2e4d2e", fg="white", relief="flat").pack(side=tk.RIGHT, padx=10)

        self.canvas = tk.Label(self.root, bg="#1a1a1a")
        self.canvas.pack(expand=True, fill=tk.BOTH)

        self.log_box = tk.Text(self.root, height=6, bg="#0a0a0a", fg="#00ff00",
                               font=("Consolas", 9), padx=10, pady=5)
        self.log_box.pack(side=tk.BOTTOM, fill=tk.X)

    # =========================
    # Folder Processing
    # =========================

    def process_folder_logic(self, src_folder, palette_name, palette_colors, skip_files=None):
        if skip_files is None:
            skip_files = []

        parent = os.path.dirname(src_folder)
        folder_name = palette_name.lower().replace(" ", "_")
        dest_folder = os.path.join(parent, f"{folder_name}.rsi")
        os.makedirs(dest_folder, exist_ok=True)

        for file in os.listdir(src_folder):
            src_file = os.path.join(src_folder, file)
            dest_file = os.path.join(dest_folder, file)

            if file in skip_files and file.lower().endswith(".png"):
                shutil.copy2(src_file, dest_file)
                continue

            if file.lower().endswith(".png"):
                with Image.open(src_file) as img:
                    new_img = apply_smart_palette(img, palette_colors)
                    new_img.save(dest_file)
            elif os.path.isfile(src_file):
                shutil.copy2(src_file, dest_file)

        return folder_name

    def mass_process_by_template(self):
        parent_folder = filedialog.askdirectory(title="Select Root Folder")
        if not parent_folder:
            return

        template_name = self.template_name.get().strip()
        if not template_name:
            messagebox.showwarning("Warning", "Template folder name is empty.")
            return

        skip_list = [f.strip() for f in self.skip_files.get().split(",") if f.strip()]
        self.log(f"Skipping files: {', '.join(skip_list) if skip_list else 'None'}")
        self.log("--- Searching for template folders recursively ---")

        def recursive_process(current_folder):
            for entry in os.scandir(current_folder):
                if entry.is_dir():
                    if entry.name.endswith(".rsi") and entry.name != template_name:
                        continue

                    if entry.name == template_name:
                        self.log(f"Found template: {entry.path}")
                        for key in self.flat_palettes:
                            name, colors = self.flat_palettes[key]
                            f_name = self.process_folder_logic(
                                entry.path, name, colors, skip_files=skip_list
                            )
                            self.log(f"Created: {f_name}.rsi")
                        continue
                    recursive_process(entry.path)

        recursive_process(parent_folder)
        self.log("--- Finished ---")
        messagebox.showinfo("Success", "All matching templates processed!")

    # =========================
    # UI Actions
    # =========================

    def load_file(self):
        path = filedialog.askopenfilename(filetypes=[("PNG Images", "*.png")])
        if not path:
            return
        self.image = Image.open(path)
        self.log(f"File loaded: {os.path.basename(path)}")
        self.update_preview()

    def update_preview(self):
        if not (self.image and self.pal_select.get()):
            return
        _, colors = self.flat_palettes[self.pal_select.get()]
        self.processed = apply_smart_palette(self.image, colors)
        self.render()

    def render(self):
        if not self.processed:
            return
        scale = self.scale.get()
        w, h = self.processed.size
        upscaled = self.processed.resize((w * scale, h * scale), Image.NEAREST)
        self.tk_img = ImageTk.PhotoImage(upscaled)
        self.canvas.config(image=self.tk_img)

    def process_folder(self):
        folder = filedialog.askdirectory(title="Select RSI Folder")
        if not folder or not self.pal_select.get():
            return
        name, colors = self.flat_palettes[self.pal_select.get()]
        result = self.process_folder_logic(folder, name, colors)
        self.log(f"Processed: {result}.rsi")
        messagebox.showinfo("Done", f"Folder {result}.rsi created!")

    def mass_process(self):
        folder = filedialog.askdirectory(title="Select Template RSI Folder")
        if not folder:
            return
        if not messagebox.askyesno("Mass Process",
                                   f"Create {len(self.flat_palettes)} folders?"):
            return
        self.log("--- Starting Mass Process ---")
        for _, (name, colors) in self.flat_palettes.items():
            result = self.process_folder_logic(folder, name, colors)
            self.log(f"Created: {result}.rsi")
            self.root.update()
        self.log("--- Finished ---")
        messagebox.showinfo("Success", "All palettes processed!")

    def save_single(self):
        if not self.processed:
            return
        path = filedialog.asksaveasfilename(defaultextension=".png")
        if not path:
            return
        self.processed.save(path)
        self.log(f"Saved: {os.path.basename(path)}")


# =========================
# Entry Point
# =========================

if __name__ == "__main__":
    root = tk.Tk()
    style = ttk.Style()
    style.theme_use("clam")
    style.configure("TCombobox", fieldbackground="#333", background="#444", foreground="white")

    app = ArmorApp(root)
    root.mainloop()
