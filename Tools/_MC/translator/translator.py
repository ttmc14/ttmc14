#!/usr/bin/env python3
"""
Translator script for MC entity prototypes.
Reads yml files from Resources/Prototypes/_MC/ with type: entity
and generates .ftl translation files in Resources/Locale/ru-RU/_MC/entities/
"""

import os
import sys
import re
from pathlib import Path
from typing import Dict, List, Optional, Tuple

# Check if PyYAML is installed
try:
    import yaml
except ImportError:
    print("Ошибка: Библиотека PyYAML не найдена.")
    print("Установите её командой: pip install PyYAML")
    print("Или запустите install.bat")
    sys.exit(1)


# Ignore unknown YAML tags (like !type:Container, !type:PhysShapeAabb, etc.)
def ignore_unknown_tags(loader, node):
    """Ignore unknown YAML tags and return None or empty values."""
    if isinstance(node, yaml.ScalarNode):
        return node.value
    elif isinstance(node, yaml.SequenceNode):
        return loader.construct_sequence(node)
    elif isinstance(node, yaml.MappingNode):
        return loader.construct_mapping(node)
    return None


yaml.SafeLoader.add_constructor(None, ignore_unknown_tags)


# Paths
SCRIPT_DIR = Path(__file__).parent
PROJECT_ROOT = SCRIPT_DIR.parent.parent.parent
PROTOTYPES_PATH = PROJECT_ROOT / "Resources" / "Prototypes" / "_MC"
LOCALE_PATH = PROJECT_ROOT / "Resources" / "Locale" / "ru-RU" / "_MC" / "entities"
LOCALE_ROOT = PROJECT_ROOT / "Resources" / "Locale" / "ru-RU"  # Root for duplicate checking

# Test mode flag
TEST_MODE = False
if len(sys.argv) > 1 and sys.argv[1] == '--test':
    TEST_MODE = True
    PROTOTYPES_PATH = SCRIPT_DIR
    print("=== ТЕСТИРОВАНИЕ ===")
    print(f"Режим тестирования включен")
    print(f"Используется папка скрипта для прототипов: {PROTOTYPES_PATH}")
    print(f"Целевая папка локализации: {LOCALE_PATH}")
    print()


def pascal_to_kebab(name: str) -> str:
    """Convert PascalCase to kebab-case for folder names."""
    # Insert hyphen before uppercase letters and lowercase
    s1 = re.sub('(.)([A-Z][a-z]+)', r'\1-\2', name)
    return re.sub('([a-z0-9])([A-Z])', r'\1-\2', s1).lower()


def camel_to_snake(name: str) -> str:
    """Convert camelCase to snake_case for file names."""
    # Insert underscore before uppercase letters and lowercase
    s1 = re.sub('(.)([A-Z][a-z]+)', r'\1_\2', name)
    return re.sub('([a-z0-9])([A-Z])', r'\1_\2', s1).lower()


def extract_entities_from_yaml(file_path: Path) -> List[Dict]:
    """Extract entity prototypes from a YAML file."""
    entities = []

    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()

        # Parse YAML documents (can be multiple documents in one file)
        docs = yaml.safe_load_all(content)

        for doc in docs:
            if doc is None:
                continue
            if isinstance(doc, list):
                for item in doc:
                    if isinstance(item, dict) and item.get('type') == 'entity':
                        entities.append(item)
            elif isinstance(doc, dict):
                if doc.get('type') == 'entity':
                    entities.append(doc)

    except Exception as e:
        print(f"Error reading {file_path}: {e}")

    return entities


def get_relative_path_parts(file_path: Path, base_path: Path) -> List[str]:
    """Get the relative path components from base_path to file_path."""
    try:
        rel_path = file_path.relative_to(base_path)
        return list(rel_path.parts)
    except ValueError:
        return []


def collect_all_entities(prototypes_path: Path) -> Dict[str, Tuple[Dict, Path]]:
    """
    Collect all entity prototypes from yml files.
    Returns a dict mapping entity ID -> (entity_data, source_file_path)
    """
    all_entities = {}
    duplicates = {}

    for yml_file in prototypes_path.rglob('*.yml'):
        entities = extract_entities_from_yaml(yml_file)

        for entity in entities:
            entity_id = entity.get('id')
            if not entity_id:
                continue

            if entity_id in all_entities:
                # Found a duplicate
                if entity_id not in duplicates:
                    duplicates[entity_id] = []
                duplicates[entity_id].append(yml_file)
                # Keep the first occurrence (or you could implement logic to prefer certain paths)
            else:
                all_entities[entity_id] = (entity, yml_file)

    # Report duplicates
    if duplicates:
        print("\n=== Дубликаты ID прототипов ===")
        for entity_id, files in duplicates.items():
            print(f"  {entity_id}:")
            print(f"    Основной: {all_entities[entity_id][1]}")
            for dup_file in files:
                print(f"    Дубль: {dup_file}")
        print()

    return all_entities


def generate_ftl_content(entity: Dict) -> List[str]:
    """Generate Fluent translation content for an entity."""
    entity_id = entity.get('id', '')
    name = entity.get('name', '')
    description = entity.get('description', '')
    suffix = entity.get('suffix', '')
    parent = entity.get('parent', '')

    lines = []

    # Handle parent - if it's a list, take the last one
    if isinstance(parent, list):
        parent = parent[-1] if parent else ''

    # Main entry: ent-<id> = <name>
    if name:
        lines.append(f"ent-{entity_id} = {name}")
    elif parent:
        # If no name, inherit from parent using Fluent reference
        lines.append(f"ent-{entity_id} = {{ ent-{parent} }}")
    else:
        # If no name and no parent, use empty Fluent reference
        lines.append(f"ent-{entity_id} = {{ \"\" }}")

    # .desc attribute
    if description:
        lines.append(f"    .desc = {description}")
    elif parent:
        # If no description but has parent, inherit from parent
        lines.append(f"    .desc = {{ ent-{parent}.desc }}")

    # .suffix attribute
    if suffix:
        lines.append(f"    .suffix = {suffix}")

    return lines


def parse_ftl_file(ftl_path: Path) -> Dict[str, List[str]]:
    """
    Parse an FTL file and return a dict of key -> lines (including attributes).
    Returns: {key: [key_line, attr_line1, attr_line2, ...], ...}
    """
    keys = {}
    current_key = None
    current_lines = []

    if not ftl_path.exists():
        return {}

    with open(ftl_path, 'r', encoding='utf-8') as f:
        for line in f:
            line = line.rstrip('\n')

            # Check if this is a key line (starts with key = or key.attr =)
            key_match = re.match(r'^([a-zA-Z0-9_-]+)\s*=', line)
            attr_match = re.match(r'^\s*\.([a-zA-Z0-9_-]+)\s*=', line)

            if key_match and not attr_match:
                # Save previous key
                if current_key is not None:
                    keys[current_key] = current_lines

                # Start new key
                current_key = key_match.group(1)
                current_lines = [line]
            elif attr_match and current_key is not None:
                # Attribute of current key
                current_lines.append(line)
            elif line.strip() == '':
                # Empty line - end of current entry
                if current_key is not None:
                    keys[current_key] = current_lines
                    current_key = None
                    current_lines = []

    # Don't forget the last key
    if current_key is not None:
        keys[current_key] = current_lines

    return keys


def has_russian_text(lines: List[str]) -> bool:
    """Check if lines contain Russian (Cyrillic) text."""
    return any('\u0400' <= c <= '\u04FF' for line in lines for c in line)


def postprocess_duplicates():
    """
    Postprocess generated FTL files to remove duplicate keys.
    - Scans entire ru-RU locale directory for duplicates
    - Priority: keep keys in entities/ directory (new generated files)
    - If Russian translation found in old file (prototypes/), copy it to new file (entities/)
    - Remove duplicate keys from OLD files (not from entities/)
    - Remove keys that don't correspond to any entity prototype (ent-<id> keys only)
    - Remove empty files and directories
    """
    print("\n=== Постпроцессинг: удаление дубликатов ===")
    print(f"Сканирование: {LOCALE_ROOT}")

    # Collect all entity IDs from prototypes
    all_entities = collect_all_entities(PROTOTYPES_PATH)
    entity_ids = set(all_entities.keys())
    print(f"  Найдено {len(entity_ids)} сущностей в прототипах")

    # Collect all keys from ALL FTL files in ru-RU directory
    all_keys: Dict[str, List[Tuple[Path, List[str]]]] = {}  # key -> [(file_path, lines), ...]
    ftl_files: Dict[Path, Dict[str, List[str]]] = {}  # file_path -> {key: lines}

    # Scan entire ru-RU directory, not just entities
    for ftl_path in sorted(LOCALE_ROOT.rglob('*.ftl')):
        keys = parse_ftl_file(ftl_path)
        if not keys:
            continue

        ftl_files[ftl_path] = keys

        for key, lines in keys.items():
            if key not in all_keys:
                all_keys[key] = []
            all_keys[key].append((ftl_path, lines))

    # Process duplicates
    files_to_update: Dict[Path, Dict[str, List[str]]] = {}  # file_path -> {key: new_lines}
    keys_to_remove: Dict[Path, List[str]] = {}  # file_path -> [keys to remove]

    for key, occurrences in all_keys.items():
        if len(occurrences) <= 1:
            continue  # No duplicate

        # Find file in entities/ directory (our generated files - preferred location)
        # Use as_posix() for cross-platform path comparison
        entities_occurrences = [(p, l) for p, l in occurrences if '_MC/entities' in p.as_posix()]
        old_occurrences = [(p, l) for p, l in occurrences if '_MC/entities' not in p.as_posix()]

        if not entities_occurrences:
            continue  # No generated file for this key, skip

        entities_path = entities_occurrences[0][0]
        entities_lines = entities_occurrences[0][1]

        # Always take the first old occurrence
        for old_path, old_lines in old_occurrences:
            if entities_path not in files_to_update:
                files_to_update[entities_path] = {}
            files_to_update[entities_path][key] = old_lines
            break

        # Remove key from ALL old files (not from entities/)
        for old_path, _ in old_occurrences:
            if old_path not in keys_to_remove:
                keys_to_remove[old_path] = []
            keys_to_remove[old_path].append(key)

    # Apply updates to entities/ files (copy Russian text from old files)
    files_updated = 0
    for ftl_path, updates in sorted(files_to_update.items()):
        keys = ftl_files[ftl_path]
        for key, new_lines in updates.items():
            keys[key] = new_lines

        # Rewrite file
        content_lines = []
        for key, lines in sorted(keys.items()):
            content_lines.extend(lines)
            content_lines.append('')

        with open(ftl_path, 'w', encoding='utf-8') as f:
            f.write('\n'.join(content_lines))

        files_updated += 1
        print(f"  Обновлено {len(updates)} ключей в {ftl_path.relative_to(LOCALE_ROOT)}")

    # Remove duplicate keys from OLD files (prototypes/, etc.)
    files_modified = 0
    files_to_delete = []

    for ftl_path, keys_to_delete in sorted(keys_to_remove.items()):
        if ftl_path not in ftl_files:
            continue

        keys = ftl_files[ftl_path]

        # Remove duplicate keys
        for key in keys_to_delete:
            del keys[key]

        # Rewrite file if it still has keys
        if keys:
            content_lines = []
            for key, lines in sorted(keys.items()):
                content_lines.extend(lines)
                content_lines.append('')

            with open(ftl_path, 'w', encoding='utf-8') as f:
                f.write('\n'.join(content_lines))

            files_modified += 1
            print(f"  Удалено {len(keys_to_delete)} дубликатов из {ftl_path.relative_to(LOCALE_ROOT)}")
        else:
            # File is empty - mark for deletion
            files_to_delete.append(ftl_path)

    # Delete empty files ONLY inside _MC directory
    dirs_to_check = set()
    for ftl_path in sorted(files_to_delete):
        # Only delete if file is inside _MC directory
        if '_MC/' not in ftl_path.as_posix() and '_MC\\' not in ftl_path.as_posix():
            continue
        ftl_path.unlink()
        dirs_to_check.add(ftl_path.parent)
        print(f"  Удалён пустой файл: {ftl_path.relative_to(LOCALE_ROOT)}")

    # Delete empty directories ONLY inside _MC directory (recursively)
    dirs_deleted = 0
    while True:
        empty_dirs = []
        for dir_path in LOCALE_ROOT.rglob('*'):
            # Only check directories inside _MC
            if '_MC/' not in dir_path.as_posix() and '_MC\\' not in dir_path.as_posix():
                continue
            if dir_path.is_dir() and not any(dir_path.iterdir()):
                empty_dirs.append(dir_path)

        if not empty_dirs:
            break

        for dir_path in sorted(empty_dirs, reverse=True):
            try:
                dir_path.rmdir()
                dirs_deleted += 1
                print(f"  Удалена пустая директория: {dir_path.relative_to(LOCALE_ROOT)}")
            except OSError:
                # Directory not empty (race condition)
                pass

    # Second pass: Remove keys that don't correspond to any entity (ONLY inside _MC)
    print("\n=== Постпроцессинг: удаление ключей без сущностей ===")
    orphaned_keys_removed = 0
    files_with_orphans = 0

    for ftl_path in sorted(LOCALE_ROOT.rglob('*.ftl')):
        # Only process files inside _MC directory
        if '_MC/' not in ftl_path.as_posix() and '_MC\\' not in ftl_path.as_posix():
            continue

        if not ftl_path.exists():
            continue

        keys = parse_ftl_file(ftl_path)
        if not keys:
            continue

        # Find ent-<id> keys that don't have corresponding entity
        keys_to_delete = []
        for key in keys:
            if key.startswith('ent-'):
                entity_id = key[4:]  # Remove 'ent-' prefix
                if entity_id not in entity_ids:
                    keys_to_delete.append(key)

        if keys_to_delete:
            # Remove orphaned keys
            for key in keys_to_delete:
                del keys[key]

            orphaned_keys_removed += len(keys_to_delete)
            files_with_orphans += 1

            # Rewrite file
            if keys:
                content_lines = []
                for key, lines in sorted(keys.items()):
                    content_lines.extend(lines)
                    content_lines.append('')

                with open(ftl_path, 'w', encoding='utf-8') as f:
                    f.write('\n'.join(content_lines))

                print(f"  Удалено {len(keys_to_delete)} ключей без сущностей из {ftl_path.relative_to(LOCALE_ROOT)}")
            else:
                # File is now empty - delete it
                ftl_path.unlink()
                print(f"  Удалён пустой файл (все ключи без сущностей): {ftl_path.relative_to(LOCALE_ROOT)}")

    print(f"\nПостпроцессинг завершён: обновлено {files_updated} файлов, изменено {files_modified} файлов, удалено {len(files_to_delete)} пустых файлов, {dirs_deleted} пустых директорий, {orphaned_keys_removed} ключей без сущностей из {files_with_orphans} файлов")


def process_entities():
    """Main processing function."""
    print(f"Сканирование прототипов из: {PROTOTYPES_PATH}")
    print(f"Целевая папка локализации: {LOCALE_PATH}")

    # Collect all entities
    all_entities = collect_all_entities(PROTOTYPES_PATH)
    print(f"Найдено {len(all_entities)} уникальных entity прототипов")

    # Group entities by their source directory structure
    entities_by_dir: Dict[str, List[Tuple[Dict, str]]] = {}

    for entity_id, (entity, source_path) in all_entities.items():
        # Get relative path from prototypes root
        rel_parts = get_relative_path_parts(source_path, PROTOTYPES_PATH)

        if not rel_parts:
            continue

        # Skip only the "Entities" directory to avoid duplicating it in the locale path
        if rel_parts and rel_parts[0].lower() == 'entities':
            rel_parts = rel_parts[1:]

        # Convert directory parts from PascalCase to kebab-case
        dir_parts = [pascal_to_kebab(part) for part in rel_parts[:-1]]
        filename_base = camel_to_snake(rel_parts[-1].replace('.yml', ''))

        # Create directory path string
        dir_path = '/'.join(dir_parts) if dir_parts else ''

        if dir_path not in entities_by_dir:
            entities_by_dir[dir_path] = []

        entities_by_dir[dir_path].append((entity, filename_base))

    # Generate translation files
    for dir_path, entities_list in sorted(entities_by_dir.items()):
        # Create output directory
        if dir_path:
            output_dir = LOCALE_PATH / dir_path
        else:
            output_dir = LOCALE_PATH

        output_dir.mkdir(parents=True, exist_ok=True)

        # Group entities by filename base
        entities_by_file: Dict[str, List[Dict]] = {}

        for entity, filename_base in entities_list:
            if filename_base not in entities_by_file:
                entities_by_file[filename_base] = []
            entities_by_file[filename_base].append(entity)

        # Generate content for each file
        for filename_base, entities in entities_by_file.items():
            ftl_filename = f"{filename_base}.ftl"
            ftl_path = output_dir / ftl_filename

            # Generate FTL content
            content_lines = []
            for entity in entities:
                entity_lines = generate_ftl_content(entity)
                content_lines.extend(entity_lines)
                content_lines.append('')  # Empty line between entries

            # Write file
            with open(ftl_path, 'w', encoding='utf-8') as f:
                f.write('\n'.join(content_lines))

            print(f"Создан: {ftl_path.relative_to(PROJECT_ROOT)} ({len(entities)} сущностей)")

    print(f"\nГотово! Обработано {len(all_entities)} сущностей в {sum(len(v) for v in entities_by_file.values())} файлах")

    # Postprocess to remove duplicate keys
    postprocess_duplicates()


if __name__ == '__main__':
    # Check if PyYAML is installed
    try:
        import yaml
    except ImportError:
        print("Ошибка: Библиотека PyYAML не найдена.")
        print("Установите её командой: pip install PyYAML")
        sys.exit(1)

    # Ensure locale directory exists
    LOCALE_PATH.mkdir(parents=True, exist_ok=True)

    process_entities()
