# Стиль C# в Hypercube

Соглашения для написания и ревью C#-кода Hypercube.

[Оформление](#оформление) · [Имена](#имена) · [Методы и свойства](#методы-и-свойства) · [Утилиты и методы расширения](#утилиты-и-методы-расширения) · [Ветвление и переходы](#ветвление-и-переходы) · [Типы и идентификаторы](#типы-и-идентификаторы) · [Интерфейсы](#интерфейсы) · [Документация и тесты](#документация-и-тесты)

## Оформление

**Файл.** Имя файла обычно совпадает с именем основного типа. Директивы `using` располагаются в начале файла, сначала `System.*`, затем пространства имён проекта и сторонних библиотек. После пустой строки следует пространство имён на уровне файла, например `namespace Hypercube.Utilities.Collections;`. Оно соответствует каталогу типа внутри проекта. При конфликте имён псевдоним указывает источник типа, например `SilkWindow`.

**Отступы и блоки.** Один уровень отступа занимает четыре пробела. Фигурные скобки типа, метода и многострочного блока стоят на отдельных строках. Для короткого `if` с единственным `return`, `throw` или `continue` допустима запись без скобок. Пустая строка отделяет начальные проверки от основного действия.

```csharp
public void Release(T value)
{
    if (Invalid(value))
        throw new ArgumentException("Invalid number to release.", nameof(value));

    _released.Push(value);
}
```

**Объявления.** У полей класса явно указаны доступность и тип. Поле, которое получает значение один раз, объявляется с `readonly`. Если тип уже виден слева, конструктор можно записать как `new()`, например `private readonly Stack<T> _released = new();`. Для локальной переменной уместен `var`, когда её тип понятен по правой части.

**Порядок членов.** Перегрузки одного метода располагаются рядом. В крупном типе связанные операции можно объединить в регионы, например `Equality` и `Operators`. Самостоятельные части типа оформляются как `partial` в файлах вида `Тип.Тема.cs`. Имя части описывает её содержание, например `World.Queries.cs`. Поля, нужные только этой части, располагаются рядом с использующими их методами.

**Инициализация и переносы.** Для пустой коллекции с известным типом подходит выражение `[]`. В длинном объявлении ограничения обобщённого типа выносятся под сигнатуру с отступом: условия для одного параметра следуют после одного `where`, через запятую. Для разных параметров удобно выделять по строке на каждый `where`. Это сохраняет связь ограничения с именем типа. [DependenciesContainer](../submodules/utilities/src/Hypercube.Utilities/Dependencies/DependenciesContainer.cs) · [DictionaryExtension](../submodules/utilities/src/Hypercube.Utilities/Extensions/DictionaryExtension.cs)

## Имена

| Элемент | Форма | Примеры |
| --- | --- | --- |
| Тип | Существительное или сочетание слов в `PascalCase` | `EntityFactory`, `WindowCreateSettings` |
| Интерфейс | Префикс `I` перед названием контракта | `IWindow`, `IBitSet`, `IComponent` |
| Метод | Действие или устоявшийся предметный термин в `PascalCase` | `SetSize`, `Release`, `Dot` |
| Свойство, открытое поле | Имя значения в `PascalCase` | `MousePosition`, `Size`, `Vector2.X` |
| Закрытое поле экземпляра | `_camelCase` | `_entityFactory`, `_released` |
| Закрытое поле `static readonly` | Обычно `PascalCase`, в том числе для общей таблицы или вычисленного значения | `InternalMapping`, `BitsPerElement` |
| Константа типа, именованное статическое значение | `PascalCase` | `Dimensionality`, `PositiveInfinity` |
| Параметр, локальная переменная | `camelCase` | `entity`, `componentMeta` |
| Параметр обобщённого типа | `T` или имя роли с префиксом `T` | `NumPool<T>`, `TKey`, `TValue` |

### Имена методов и событий

Имя читается вместе с получателем и аргументами. У `window.SetSize(size)` получатель уже указывает на окно. У `backend.WindowSetSize(window, size)` слово `Window` уточняет, к какому объекту относится операция. Для однотипных операций сохраняется один словарь во всём семействе методов.

| Форма | Смысл в месте вызова | Пример |
| --- | --- | --- |
| `Is…`, `Has…` | Проверка состояния или наличия, результат — `bool` | `IsKeyHeld(key)`, `Has<T>(entity)` |
| `Get…` | Получение значения, которое ожидается в данном контексте | `Get<T>(entity)` |
| `TryGet…` | Отсутствие значения предусмотрено контрактом. Метод возвращает `bool` и передаёт результат через `out` | `TryGet(type, out var meta)` |
| `GetOr…` | Имя указывает, что произойдёт при отсутствии значения | `GetOrInstantiate(key)` |
| `From…` | Создание значения из указанного представления или единицы измерения | `Angle.FromDegrees(degrees)` |
| `As…` | Доступ к значению в указанном представлении. Форма результата уточняется типом возврата | `AsArray()`, `AsReadOnlySpan()` |
| `…Fast`, `…Unsafe…` | Особые свойства варианта операции: приближённое вычисление или доступ с ослабленными гарантиями | `LengthFast`, `AsUnsafeSpan()` |
| `On…` у события | Уведомление, доступное подписчикам | `window.OnSize`, `window.OnClose` |

### Суффиксы классов

Первая часть имени называет предмет, суффикс уточняет роль класса. Например, `ComponentRegistry` обозначает реестр компонентов. Для самостоятельного предметного понятия достаточно имени без суффикса, например `Window`.

| Суффикс | Какую роль обозначает | Примеры |
| --- | --- | --- |
| `Helper` | Общие операции над одним предметом, сгруппированные в статическом классе | [PathHelper](../submodules/utilities/src/Hypercube.Utilities/Helpers/PathHelper.cs) |
| `Extension`, `Extensions` | Статический класс с методами расширения для типа или семейства типов | [DictionaryExtension](../submodules/utilities/src/Hypercube.Utilities/Extensions/DictionaryExtension.cs), [StringBuilderExtensions](../submodules/utilities/src/Hypercube.Utilities/Extensions/StringBuilderExtensions.cs) |
| `Factory` | Создание экземпляра, в том числе выбор реализации по параметрам | [ProcessorFactory](../submodules/windowing/src/Hypercube.Windowing/Backend/ProcessorFactory.cs) |
| `Registry` | Регистрация и поиск записей по ключу | [ComponentRegistry](../submodules/ecs/src/Hypercube.Ecs/Components/ComponentRegistry.cs) |
| `Pool` | Управление хранилищем однотипных элементов или повторная выдача освобождённых значений | [ComponentPool](../submodules/ecs/src/Hypercube.Ecs/Components/Pool/ComponentPool.cs), [NumPool](../submodules/utilities/src/Hypercube.Utilities/Collections/NumPool.cs) |
| `Parser` | Разбор входных данных по правилам их формата | [ArgumentParser](../submodules/utilities/src/Hypercube.Utilities/Arguments/ArgumentParser.cs) |
| `Formatter` | Формирование текстового представления значения | [MatrixFormatter](../submodules/mathematics/src/Hypercube.Mathematics/Matrices/Utilities/MatrixFormatter.cs) |
| `Backend` | Реализация общего контракта поверх конкретной библиотеки | [GlfwBackend](../submodules/windowing/src/Hypercube.Windowing.Glfw/GlfwBackend.cs), [SdlBackend](../submodules/windowing/src/Hypercube.Windowing.Sdl/SdlBackend.cs) |
| `Processor` | Обработка поступающих данных или операций, например оконного ввода | [WindowingInputProcessor](../submodules/windowing/src/Hypercube.Windowing/Core/Input/WindowingInputProcessor.cs) |
| `Attribute` | Атрибут C#. В месте применения допускается краткая форма `[IdStruct(typeof(int))]` | [IdStructAttribute](../submodules/utilities/src/Hypercube.Utilities/Attributes/IdStructAttribute.cs) |
| `Generator` | Генератор исходного кода для компилятора | [IdStructGenerator](../submodules/utilities/src/Hypercube.Utilities.Analyzers/Generators/IdStructGenerator.cs) |
| `Analyzer` | Анализ кода и выдача диагностик | [DependencyAnalyzer](../submodules/utilities/src/Hypercube.Utilities.Analyzers/DependencyAnalyzer.cs) |
| `Exception` | Тип исключения, имя которого отражает причину ошибки | [CircularDependencyException](../submodules/utilities/src/Hypercube.Utilities/Dependencies/Exceptions/CircularDependencyException.cs) |

### Имена в строках

Если строка обозначает имя класса или поля, **обязателен `nameof`**. Например, `nameof(BitSet)` и `nameof(BitSet.Size)` дают строки `"BitSet"` и `"Size"`. Записывать такие имена строковыми литералами запрещено. Ссылка на символ позволяет компилятору проверить имя и сохраняет связь с объявлением при переименовании в IDE. Для имени параметра в исключении действует тот же приём — `nameof(value)`.

## Методы и свойства

**Свойство обозначает значение, метод — действие.** Например, `Size` представляет известный размер, а `SetSize(size)` выражает запрос на изменение. Если чтение свойства меняет состояние, этот эффект явно описывается в его документации.

**Перегрузки имеют общую реализацию.** Короткая перегрузка подставляет значение по умолчанию или текущий контекст и передаёт управление основной. Для одного выражения, в том числе делегирующего вызова, подходит запись через `=>`. Для последовательности проверок и действий используется блок.

**Способ передачи данных виден в сигнатуре.** Параметр `out` передаёт дополнительный результат, `in T` обозначает доступ к аргументу по ссылке только для чтения, а возврат `ref T` даёт доступ к месту хранения. Запись `ref var component = ref world.Get<T>(entity);` сохраняет ссылку. Обычное присваивание структуры создаёт копию.

### Методы With и изменяемость

У неизменяемого типа метод `With…` возвращает новое значение с указанным изменением. У изменяемого построителя такой метод обновляет текущее состояние и может вернуть `this` или `ref this` для цепочки вызовов. По одному префиксу `With` нельзя заключить, что создана копия. [Vector2](../submodules/mathematics/src/Hypercube.Mathematics/Vectors/Vector2.cs) · [QueryMeta](../submodules/ecs/src/Hypercube.Ecs/Queries/QueryMeta.cs)

```csharp
var moved = position.WithX(10f);  // position сохраняет прежние координаты.

var meta = new QueryMeta();
meta.WithAll<T>();                // Условие уже записано в meta.
```

### Контракты методов

Для непрерывного участка данных `ReadOnlySpan<T>` выражает доступ только для чтения, `Span<T>` — доступ с возможностью записи. Владение памятью и создание копии — отдельные свойства контракта. Например, `Vector2.AsArray()` создаёт массив, а `AsSpan()` предоставляет представление тех же координат. Если запись через представление обходит неизменяемость типа, это отражается в имени и документации метода, как у `AsUnsafeSpan()`. [Vector2](../submodules/mathematics/src/Hypercube.Mathematics/Vectors/Vector2.cs) · [SpanBitSet](../submodules/utilities/src/Hypercube.Utilities/Collections/Bit/SpanBitSet.cs)

Когда допустимость `null` у выходного параметра зависит от результата метода, связь можно указать в сигнатуре. Сочетание `bool` и `[NotNullWhen(true)] out IMonitor? monitor` сообщает компилятору, что в успешной ветви ссылка определена. [TryGetMonitor](../submodules/windowing/src/Hypercube.Windowing/Core/Device/WindowingDevice.Monitors.cs)

**Встраивание коротких методов.** Для небольшой операции на часто выполняемом пути подходит `[MethodImpl(MethodImplOptions.AggressiveInlining)]`. У свойства атрибут располагается над конкретным аксессором. Это подсказка среде выполнения, поэтому краткость метода сама по себе не требует атрибута. [Vector2.Length](../submodules/mathematics/src/Hypercube.Mathematics/Vectors/Vector2.cs)

### События и подписки

Тип аргументов события обычно получает суффикс `Args`, именованный делегат обработчика — `Handler`. Отдельный делегат сохраняет в контракте имена параметров и при необходимости модификатор `ref`. Для простого уведомления подходит `event Action?`, для передачи значения — `event Action<T>?`. Подписка и отписка образуют пару с одинаковым предметом в имени, например `SubscribeWindow` и `UnsubscribeWindow`. Объект, который подписывается на долгоживущий источник, снимает свои обработчики при завершении работы. [KeyChangedArgs](../submodules/windowing/src/Hypercube.Windowing/Core/Input/Args/KeyChangedArgs.cs) · [WindowSizeHandler](../submodules/windowing/src/Hypercube.Windowing/Backend/Handlers/WindowSizeHandler.cs) · [EntityRefAction](../submodules/ecs/src/Hypercube.Ecs/Entities/EntityRefAction.cs) · [Window](../submodules/windowing/src/Hypercube.Windowing/Core/Windows/Window.cs)

## Утилиты и методы расширения

**Общая логика выносится в один метод.** Если нескольким типам нужна одна формула, проверка или процедура форматирования, они вызывают общую реализацию. Различия в данных передаются параметрами. Например, общий форматтер матриц получает элементы и размерность матрицы. [MatrixFormatter](../submodules/mathematics/src/Hypercube.Mathematics/Matrices/Utilities/MatrixFormatter.cs)

**Метод расширения добавляет удобную форму вызова.** Если операция естественно читается как действие над значением, это значение становится первым параметром с модификатором `this`. Когда алгоритм уже выделен в утилиту, расширение делегирует ей вычисление. Так формула и значение допуска по умолчанию имеют один источник. [FloatingPointEqualsExtension](../submodules/mathematics/src/Hypercube.Mathematics/Extensions/FloatingPointEqualsExtension.cs)

```csharp
public static bool AboutEquals(this float value, float other, float tolerance = HyperMath.FloatTolerance)
    => HyperMath.AboutEquals(value, other, tolerance);
```

**Область применения определяет размещение.** Операция, нужная одному классу, остаётся его закрытым методом. Общая утилита располагается в модуле, которому принадлежит задача. Для расширения существенны и тип получателя, и типы аргументов: например, операция над набором битов, принимающая метаданные компонентов, относится к ECS. [BitSetExtension](../submodules/ecs/src/Hypercube.Ecs/Utilities/Extensions/BitSetExtension.cs)

## Ветвление и переходы

**Ветви `else` и цепочки `if / else if` крайне нежелательны.** Для случая с известным результатом предпочтителен ранний `return`, для нарушения предусловия — `throw`, для пропуска элемента в цикле — `continue`. После коротких проверок основной код остаётся на одном уровне отступа.

```csharp
if (_queries.TryGetValue(meta, out var query))
    return query;

query = new Query(this, meta);
_queries[meta] = query;
return query;
```

Когда условие выбирает одно из двух коротких выражений, подходит `?:`. Несколько вариантов одного значения удобно представить через `switch` или `switch`-выражение.

**Проверка на `null`** записывается через `value is null` и `value is not null`. Сравнения `== null` и `!= null` для этой цели не используются. Проверка через `is` сохраняет смысл и у типов с перегруженными операторами равенства.

**Оператор `goto` запрещён**, включая переход к метке, `goto case` и `goto default`. Выход из цикла или секции `switch` выражается через `break`. Общие действия нескольких ветвей могут быть вынесены в метод.

## Типы и идентификаторы

**Классы и записи.** Для конкретной реализации без предусмотренных точек наследования подходит `sealed class`. Для компактного сообщения или набора данных с равенством по значениям — `readonly record struct`. У позиционной записи параметры, которые образуют публичные свойства, именуются в `PascalCase`, например `CommandWindowSetSize(WindowHandle Window, Vector2i Size)`. [EventBus](../submodules/ecs/src/Hypercube.Ecs/Events/EventBus.cs) · [CommandWindowSetSize](../submodules/windowing/src/Hypercube.Windowing/Backend/Commands/Window/CommandWindowSetSize.cs)

**Флаговые перечисления.** Набор совместимых признаков оформляется как `[Flags]`-перечисление. Пустое значение называется `None` и равно нулю. Каждому независимому признаку соответствует отдельный бит, например `Read = 1 << 0`. Именованные комбинации составляются из этих признаков: `ReadWrite = Read | Write`. [Permissions](../submodules/utilities/src/Hypercube.Utilities/Permissions.cs) · [KeyModifiers](../submodules/windowing/src/Hypercube.Windowing/Input/KeyModifiers.cs)

**Неизменяемые значения.** Для небольшого значения, которое целиком определяется своими данными, подходит `readonly struct`. Постоянные составляющие такого значения могут быть открытыми полями `readonly`. Для настроек с именованной инициализацией подходят свойства `get; init;`, а значения по умолчанию размещаются рядом с их объявлениями. Необязательная ссылка отмечается `?`.

**Типизированные идентификаторы.** Идентификатор имеет предметный тип в полях, параметрах и ключах коллекций. Идентификаторы с одинаковым числовым представлением остаются разными типами. Например, метод с параметром `EntityId` не принимает переменную типа `ComponentId`, хотя оба идентификатора хранят `int`. Суффикс `Id` обозначает идентификатор, `Handle` — дескриптор ресурса. [EntityId](../submodules/ecs/src/Hypercube.Ecs/EntityId.cs) · [WindowHandle](../submodules/windowing/src/Hypercube.Windowing/Core/Windows/WindowHandle.cs)

```csharp
[IdStruct(typeof(int))]
public readonly partial struct EntityId;
```

`IdStruct` задаёт тип хранимого значения. Общая реализация поля `Value`, конструктора, равенства, хеширования и преобразований создаётся генератором. В собственном объявлении остаются члены с предметным смыслом, например `WindowHandle.Null`. Для идентификатора на основе `int` доступны неявные преобразования в обе стороны, поэтому тип параметра особенно важен при передаче ID между методами. [IdStructGenerator](../submodules/utilities/src/Hypercube.Utilities.Analyzers/Generators/IdStructGenerator.cs)

**Преобразования значений.** Неявное преобразование (`implicit`) подходит для взаимозаменяемых представлений, например пары координат и вектора той же размерности. Преобразование с возможной потерей данных выражается явно (`explicit`), как переход от `Vector2` к целочисленному `Vector2i`. Для нескольких способов создать одно значение перегрузки конструктора могут делегировать инициализацию основной форме через `: this(...)`. [Vector2](../submodules/mathematics/src/Hypercube.Mathematics/Vectors/Vector2.cs)

## Интерфейсы

**Контракт описывает операции, доступные вызывающему коду.** У числового типа интерфейс может выражать отдельную возможность, например `IAdditionOperators<Vector2, Vector2, Vector2>` делает сложение доступным обобщённому алгоритму. В длинном объявлении двоеточие остаётся после имени типа, а каждый интерфейс занимает отдельную строку с отступом. [Vector2](../submodules/mathematics/src/Hypercube.Mathematics/Vectors/Vector2.cs)

**Публичная реализация** подходит для операции, которая входит в обычный API типа. **Явная реализация** подходит для дополнительной формы той же операции, требуемой интерфейсом. Например, типизированный перечислитель задаёт порядок обхода один раз, а нетипизированный вызывает его напрямую.

```csharp
IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

public IEnumerator<float> GetEnumerator()
{
    yield return X;
    yield return Y;
}
```

Та же форма применима к статическим членам. Если публичное поле уже называется `MinValue`, явное свойство `IMinMaxValue<Vector2>.MinValue` может вернуть это поле. Привычное имя остаётся доступным у типа, а обобщённый код получает значение через контракт интерфейса.

## Документация и тесты

**Атрибуты API.** Для обозначения типов и членов, предназначенных внешним потребителям библиотеки, используется `[PublicAPI]` из JetBrains.Annotations. Для типа-значения с компактным представлением в отладчике подходит `[DebuggerDisplay]`. Атрибуты располагаются непосредственно над объявлением после XML-комментария. [Vector2](../submodules/mathematics/src/Hypercube.Mathematics/Vectors/Vector2.cs) · [BitSet](../submodules/utilities/src/Hypercube.Utilities/Collections/Bit/BitSet.cs)

**Публичный API документируется на английском.** В `<summary>` описывается наблюдаемое поведение. В `<returns>` уточняется смысл результата, в `<exception>` — условие ошибки. Значимый побочный эффект указывается явно, например повторная выдача освобождённого номера при чтении свойства. Для реализации документированного интерфейса подходит `/// <inheritdoc/>`.

**Имя теста выражает проверяемое поведение.** Например, `NextPrefersReleasedNumbersLifoOrder` указывает, какой порядок выдачи проверяется. Написание имени следует стилю соседних тестов, в том числе принятому в наборе разделению слов подчёркиваниями. В NUnit одиночный сценарий оформляется через `[Test]`, а одна проверка для нескольких наборов данных — через `[TestCaseSource(nameof(...))]`. [NumPoolTests](../submodules/utilities/src/Hypercube.Utilities.UnitTests/Collections/NumPoolTests.cs) · [Тесты индексатора](../submodules/mathematics/src/Hypercube.Mathematics.UnitTests/Vectors/Vector2Tests/Indexer.cs)
