import tkinter as tk
from tkinter import filedialog, ttk
from PIL import Image, ImageTk
import os

DEFAULT_ATTACHMENTS_DIR = "../Objects/Weapons/Guns/Attachments/"
ZOOM_STEP = 1.1
MIN_ZOOM = 0.2
MAX_ZOOM = 8.0

class SpriteApp:
    def __init__(self, root):
        self.root = root
        self.root.title("Sprite Attachment Editor")

        self.base_image = None
        self.base_tk = None
        self.zoom = 1.0

        self.library = []

        # ОДИН слой на слот
        self.slot_layers = {}  # slot -> layer

        # слоты с дефолтными позициями (в пикселях, из /32)
        self.slots = {
            "muzzle": [26, 1],
            "underbarrel": [-8, -18],
            "stock": [-20, 0],
            "barrel": [13, 7],
            "rail": [-11, 7]
        }
        self.active_slot = "muzzle"

        self.setup_ui()
        self.load_library_recursive(DEFAULT_ATTACHMENTS_DIR)

    def setup_ui(self):
        self.root.configure(bg="#2b2b2b")
        style = ttk.Style()
        style.theme_use("clam")

        frame = ttk.Frame(self.root)
        frame.pack(fill="both", expand=True)

        left = ttk.Frame(frame)
        left.pack(side="left", fill="y")

        center = ttk.Frame(frame)
        center.pack(side="left", fill="both", expand=True)

        right = ttk.Frame(frame)
        right.pack(side="right", fill="y")

        ttk.Button(left, text="Load Base Sprite", command=self.load_base).pack(pady=5)
        ttk.Button(left, text="Reload Library", command=self.reload_library).pack(pady=5)

        ttk.Label(left, text="Slot:").pack()
        self.slot_box = ttk.Combobox(left, values=list(self.slots.keys()))
        self.slot_box.set(list(self.slots.keys())[0])
        self.slot_box.current(0)
        self.slot_box.bind("<<ComboboxSelected>>", self.change_slot)
        self.slot_box.pack()

        ttk.Label(left, text="Library:").pack()
        self.library_list = tk.Listbox(left, width=30, height=10, bg="#1e1e1e", fg="white")
        self.library_list.pack(fill="y")
        self.library_list.bind("<<ListboxSelect>>", self.preview_library)

        self.preview_label = ttk.Label(left)
        self.preview_label.pack(pady=5)

        ttk.Button(left, text="Apply To Slot", command=self.apply_to_slot).pack(pady=5)

        self.canvas = tk.Canvas(center, bg="#202020")
        self.canvas.pack(fill="both", expand=True)

        self.canvas.bind("<Button-1>", self.on_click)
        self.canvas.bind("<B1-Motion>", self.drag_slot)
        self.canvas.bind("<MouseWheel>", self.on_zoom)
        self.canvas.bind("<Button-4>", self.on_zoom)
        self.canvas.bind("<Button-5>", self.on_zoom)

        # стрелки двигают слот
        self.canvas.bind("<Left>", lambda e: self.move_slot(-1, 0))
        self.canvas.bind("<Right>", lambda e: self.move_slot(1, 0))
        self.canvas.bind("<Up>", lambda e: self.move_slot(0, -1))
        self.canvas.bind("<Down>", lambda e: self.move_slot(0, 1))

        self.coord_label = ttk.Label(right, text="Coords")


        ttk.Button(right, text="Copy Offsets", command=self.copy_offsets).pack(pady=10)
        self.coord_label.pack(padx=10, pady=10)

    def load_base(self):
        path = filedialog.askopenfilename(filetypes=[("Images", "*.png;*.jpg")])
        if not path:
            return

        self.base_image = Image.open(path)
        self.zoom = 1.0
        self.redraw_all()

    # -------- Library --------
    def load_library_recursive(self, folder):
        if not os.path.exists(folder):
            return

        for root_dir, _, files in os.walk(folder):
            for file in files:
                if file.lower().endswith((".png", ".jpg")):
                    self.add_to_library(os.path.join(root_dir, file))

    def reload_library(self):
        self.library.clear()
        self.library_list.delete(0, "end")
        self.load_library_recursive(DEFAULT_ATTACHMENTS_DIR)

    def add_to_library(self, path):
        image = Image.open(path)
        preview = image.resize((64, 64), Image.NEAREST)
        preview_tk = ImageTk.PhotoImage(preview)

        name = os.path.relpath(path, DEFAULT_ATTACHMENTS_DIR)
        self.library.append({"image": image, "name": name, "preview": preview_tk})
        self.library_list.insert("end", name)

    def preview_library(self, event):
        selection = self.library_list.curselection()
        if not selection:
            return
        self.preview_label.config(image=self.library[selection[0]]["preview"])

    # -------- Slots logic --------
    def apply_to_slot(self):
        selection = self.library_list.curselection()
        if not selection:
            return

        lib_item = self.library[selection[0]]
        x, y = self.slots[self.active_slot]

        # заменяем слой
        self.slot_layers[self.active_slot] = {
            "image": lib_item["image"],
            "x": x,
            "y": y,
            "name": lib_item["name"]
        }

        self.redraw_all()

    def change_slot(self, event):
        self.active_slot = self.slot_box.get()

    def move_slot(self, dx, dy):
        slot = self.slots[self.active_slot]
        slot[0] += dx
        slot[1] += dy

        # синхронизируем слой
        if self.active_slot in self.slot_layers:
            self.slot_layers[self.active_slot]["x"] = slot[0]
            self.slot_layers[self.active_slot]["y"] = slot[1]

        self.redraw_all()
        self.update_coords()

    def on_click(self, event):
        self.canvas.focus_set()

    def drag_slot(self, event):
        w = self.canvas.winfo_width()
        h = self.canvas.winfo_height()
        ox = w // 2
        oy = h // 2

        # корректное преобразование координат
        x = int((event.x - ox) / self.zoom)
        y = int((event.y - oy) / self.zoom)

        self.slots[self.active_slot][0] = x
        self.slots[self.active_slot][1] = y

        if self.active_slot in self.slot_layers:
            self.slot_layers[self.active_slot]["x"] = x
            self.slot_layers[self.active_slot]["y"] = y

        self.redraw_all()
        self.update_coords()

    # -------- Zoom --------
    def on_zoom(self, event):
        if event.delta > 0 or getattr(event, 'num', None) == 4:
            self.zoom *= ZOOM_STEP
        else:
            self.zoom /= ZOOM_STEP

        self.zoom = max(MIN_ZOOM, min(MAX_ZOOM, self.zoom))
        self.redraw_all()

    # -------- Render --------
    def redraw_all(self):
        self.canvas.delete("all")

        w = self.canvas.winfo_width()
        h = self.canvas.winfo_height()
        ox = w // 2
        oy = h // 2

        # --- GRID (32px world grid from origin) ---
        grid_step = 32
        if self.zoom > 0:
            half_w = w // (2 * self.zoom)
            half_h = h // (2 * self.zoom)

            max_x = int(half_w // grid_step + 2)
            max_y = int(half_h // grid_step + 2)

            for i in range(-max_x, max_x + 1):
                wx = i * grid_step
                x = int(wx * self.zoom) + ox
                self.canvas.create_line(x, 0, x, h, fill="#333333")

            for i in range(-max_y, max_y + 1):
                wy = i * grid_step
                y = int(wy * self.zoom) + oy
                self.canvas.create_line(0, y, w, y, fill="#333333")

        # --- BASE (centered correctly) ---
        if self.base_image:
            bw = int(self.base_image.width * self.zoom)
            bh = int(self.base_image.height * self.zoom)
            img = self.base_image.resize((bw, bh), Image.NEAREST)
            self.base_tk = ImageTk.PhotoImage(img)

            self.canvas.create_image(ox, oy, anchor="center", image=self.base_tk)

        # --- LAYERS (FIXED: true center pivot) ---
        for slot, layer in self.slot_layers.items():
            img = layer["image"].resize(
                (int(layer["image"].width * self.zoom), int(layer["image"].height * self.zoom)),
                Image.NEAREST
            )
            layer["tk"] = ImageTk.PhotoImage(img)

            x = int(layer["x"] * self.zoom) + ox
            y = int(layer["y"] * self.zoom) + oy

            # IMPORTANT: center anchor removes all offset drift
            self.canvas.create_image(x, y, anchor="center", image=layer["tk"])

        # --- SLOT MARKERS (ON TOP) ---
        for name, (x, y) in self.slots.items():
            zx = int(x * self.zoom) + ox
            zy = int(y * self.zoom) + oy

            if name == self.active_slot:
                color = "green"
                fill = "green"
            else:
                color = "red"
                fill = ""

            self.canvas.create_rectangle(
                zx - 3, zy - 3, zx + 3, zy + 3,
                outline=color,
                fill=fill,
                width=2
            )

    # -------- Coords --------
    def copy_offsets(self):
        lines = []
        lines.append("- type: AttachableHolderVisuals")
        lines.append("  offsets:")

        for name, (x, y) in self.slots.items():
            tx = x / 32
            ty = -y / 32  # inverted Y for export
            lines.append(f"    mc-slot-{name}: {tx:.6f}, {ty:.6f}")

        result = "\r\n".join(lines)

        self.root.clipboard_clear()
        self.root.clipboard_append(result)
        self.root.update()

    def update_coords(self):
        text = []
        for name, (x, y) in self.slots.items():
            text.append(f"{name}: {x}px {y}px ({x/32:.4f} / {y/32:.4f})")

        self.coord_label.config(text="\n".join(text))


if __name__ == "__main__":
    root = tk.Tk()
    app = SpriteApp(root)
    root.mainloop()
