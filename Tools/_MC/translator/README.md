# Translator для MC Entity Прототипов

Скрипт для автоматического создания файлов локализации из YAML прототипов с `type: entity`.

## Быстрый старт (Windows)

1. Запустите `install.bat` для установки зависимостей
2. Запустите `run.bat` для конвертации прототипов

**Примечание:** Скрипты автоматически проверяют работоспособность `python`, и если она не работает, используют `py` (Python Launcher).

## Установка зависимостей вручную

### Через batch файл
```
install.bat
```

### Через pip
```bash
pip install -r requirements.txt
```

## Запуск

### Windows (через batch файл - рекомендуется)
```
run.bat
```
Скрипт автоматически использует `python` или `py` в зависимости от доступности.

### Windows (PowerShell вручную)
Если у вас Python Launcher (`py`):
```powershell
cd Tools\_MC\translator
py -m pip install PyYAML
py translator.py
```

Или если у вас стандартный Python:
```powershell
cd Tools\_MC\translator
python -m pip install PyYAML
python translator.py
```

### Linux/macOS
```bash
cd Tools/_MC/translator
python3 translator.py
```

### Режим тестирования
Для тестирования с файлом `test_entities.yml` в папке скрипта:
```bash
python translator.py --test
```

## Что делает скрипт

1. Сканирует все `.yml` файлы в папке `Resources/Prototypes/_MC/`
2. Извлекает все прототипы с `- type: entity`
3. Проверяет на дубликаты ID и ключей перевода
4. Создаёт аналогичную иерархию папок в `Resources/Locale/ru-RU/_MC/entities/`
5. Конвертирует имена:
   - Папки: PascalCase → kebab-case (например, `Tier1` → `tier1`)
   - Файлы: camelCase → snake_case (например, `defender.yml` → `defender.ftl`)
6. Генерирует `.ftl` файлы с переводами в формате:
   ```
   ent-<ID> = <name>
       .desc = <description>
       .suffix = <suffix>
   ```

## Обработка дублей

Если обнаруживаются дубликаты ID прототипов:
- Первый найденный ID сохраняется как основной
- Дубликаты логируются в консоль
- Если ключ перевода дублируется в неправильной директории, используется перевод из правильной директории

## Пример

Для прототипа в `Resources/Prototypes/_MC/Actions/Xeno/Tier1/defender.yml`:
```yaml
- type: entity
  id: MCActionXenoCrestDefense
  name: Crest Defense
  description: Raises your armor values...
```

Скрипт создаст `Resources/Locale/ru-RU/_MC/entities/actions/xeno/tier1/defender.ftl`:
```
ent-MCActionXenoCrestDefense = Crest Defense
    .desc = Raises your armor values...
```
