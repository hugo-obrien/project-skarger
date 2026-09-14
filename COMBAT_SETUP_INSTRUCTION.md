# Инструкция по настройке боевой системы RTWP (Real-Time With Pause)

## Часть 1: Создание префаба снаряда для дальнего боя

### Шаг 1.1: Создание базового префаба снаряда
1. В окне **Project** перейдите в папку `Assets/_Project/Prefabs`
2. Кликните правой кнопкой мыши → **Create → Empty GameObject**
3. Назовите его `CombatProjectile`
4. Перетащите созданный GameObject из Hierarchy в папку `Prefabs` для создания префаба
5. Удалите GameObject из сцены (он теперь в префабах)

### Шаг 1.2: Настройка компонентов снаряда
1. Откройте префаб `CombatProjectile` (двойной клик в Project окне)
2. Добавьте компонент **Sphere Collider**:
   - **Is Trigger**: ✓ (галочка включена)
   - **Radius**: 0.5
   
3. Добавьте компонент **CombatProjectile** (скрипт):
   - **Visual Transform**: перетащите сам объект CombatProjectile из иерархии префаба
   - **Rotation Speed**: 720
   - **Damage**: 8 (или другое значение)

### Шаг 1.3: Настройка тега для снаряда
1. Меню **Edit → Project Settings → Tags and Layers**
2. В разделе **Tags** нажмите **+** 
3. Добавьте тег `Projectile`
4. Вернитесь к префабу `CombatProjectile`
5. Вверху Inspector найдите dropdown **Tag** и выберите `Projectile`

### Шаг 1.4: Сохранение префаба
1. Нажмите **Ctrl+S** или кнопку **Save** вверху окна Prefab Mode
2. Выйдите из режима префаба (кликните стрелку назад вверху)

---

## Часть 2: Настройка Animator Controller для боевых анимаций

### Шаг 2.1: Проверка имеющегося контроллера
1. Перейдите в `Assets/_Project/Animations`
2. Найдите `UnitAnimationController.controller`
3. Дважды кликните для открытия окна **Animator**

### Шаг 2.2: Добавление параметров аниматора
В окне **Animator**, в панели **Parameters** (слева внизу):

1. Добавьте **Bool** параметр:
   - Name: `InCombat`
   
2. Добавьте **Trigger** параметр:
   - Name: `Attack`

### Шаг 2.3: Настройка состояний анимации
Если у вас ещё не настроен контроллер с боевыми анимациями:

1. **Создайте состояния** (правый клик в Animator window → Create State → From New Blend Tree или Empty):
   - `Idle` (обычный покой)
   - `Walk` (ходьба)
   - `Run` (бег)
   - `IdleCombat` (боевая стойка)
   - `AttackMelee` (анимация атаки ближнего боя)
   - `AttackRanged` (анимация стрельбы)

2. **Назначьте анимационные клипы**:
   - Для `IdleCombat`: используйте `Character/Art/Animations/Combat/IdleCombat`
   - Для `AttackMelee`: используйте `Character/Art/Animations/Combat/PunchLeft`
   
3. **Настройте переходы** (Transitions):
   - Правый клик на состоянии → **Make Transition**
   - Пример: `Any State → IdleCombat` с условием `InCombat = true`
   - Пример: `IdleCombat → AttackMelee` по триггеру `Attack`
   - Пример: `AttackMelee → IdleCombat` (без условий, Has Exit Time = true)

### Шаг 2.4: Сохранение контроллера
1. **Ctrl+S** для сохранения
2. Убедитесь, что контроллер назначен на префабе юнита (см. Часть 3)

---

## Часть 3: Настройка префаба Юнита с боевой системой

### Шаг 3.1: Открытие префаба юнита
1. Перейдите в `Assets/_Project/Prefabs`
2. Откройте `Unit.prefab` (двойной клик)

### Шаг 3.2: Проверка базовой структуры
Убедитесь, что у префаба есть:
- **Unit** (компонент скрипта)
- **NavMesh Agent**
- **Animator** (на дочернем объекте с моделью персонажа)
- **Capsule Collider** (root collider)

### Шаг 3.3: Добавление компонента UnitCombat
1. В режиме префаба нажмите **Add Component**
2. Найдите и добавьте **UnitCombat**

### Шаг 3.4: Настройка UnitCombat
Заполните поля в Inspector:

**References:**
- **Animator**: перетащите объект с Animator (обычно дочерний объект с моделью)
- **Attack Spawn Point**: создайте пустой дочерний объект на уровне груди/плеча персонажа (для спавна снарядов), назовите его `AttackSpawnPoint`, перетащите сюда

**Animation Parameters:**
- **Combat State Parameter**: `InCombat` (должен совпадать с параметром в Animator)
- **Attack Trigger Parameter**: `Attack`
- **Idle Combat Clip Name**: `IdleCombat` (опционально, для отладки)
- **Punch Left Clip Name**: `PunchLeft` (опционально, для отладки)

**Combat Stats:**
Раскройте вкладку **Combat Stats**:

*Melee Combat:*
- **Melee Range**: 1.5 (метров)
- **Melee Damage**: 10
- **Attack Speed**: 1.5 (атак в секунду)
- **Attack Cooldown**: 0.2 (дополнительная задержка)

*Ranged Combat:*
- **Ranged Min Range**: 3 (минимальная дистанция)
- **Ranged Max Range**: 15 (максимальная дистанция)
- **Ranged Damage**: 8
- **Ranged Attack Speed**: 1 (атак в секунду)
- **Ranged Attack Cooldown**: 0.3

*Projectile:*
- **Projectile Prefab**: перетащите префаб `CombatProjectile` из `Assets/_Project/Prefabs`
- **Projectile Spawn Point**: можно оставить пустым (будет использоваться Attack Spawn Point)
- **Projectile Speed**: 10
- **Projectile Lifetime**: 5

*Behavior:*
- **Aggro Range**: 10 (радиус обнаружения врага)
- **Deaggro Range**: 15 (радиус потери аггро)
- **Chase Range**: 20 (максимальная дистанция преследования)

**Debug:**
- **Debug Gizmos**: ✓ (включено для отладки, видно радиусы в редакторе)

### Шаг 3.5: Настройка Animator на юните
1. Найдите компонент **Animator** на префабе
2. Убедитесь, что **Controller** назначен (`UnitAnimationController`)
3. Убедитесь, что **Apply Root Motion**: false (если не используется root motion)

### Шаг 3.6: Сохранение префаба юнита
1. **Ctrl+S** или кнопка **Save**
2. Выйдите из режима префаба

---

## Часть 4: Настройка слоёв и тегов для Raycast

### Шаг 4.1: Проверка слоёв
1. **Edit → Project Settings → Tags and Layers**
2. Убедитесь, что есть слои:
   - **Layer 8**: `Ground` (для земли)
   - **Layer 9**: `Unit` (для юнитов)
   - **Layer 10**: `Obstacle` (для препятствий)

### Шаг 4.2: Назначение слоёв объектам
**Для земли/террейна:**
1. Выберите ваш террейн или плоскость земли на сцене
2. Вверху Inspector найдите dropdown **Layer**
3. Выберите `Ground`
4. Если слоя нет → **Add Layer...** → создайте

**Для юнитов:**
1. Откройте `Unit.prefab`
2. В режиме префаба выберите корневой объект
3. Установите **Layer**: `Unit`
4. При сохранении выберите **Yes, change children**

---

## Часть 5: Настройка GameController и RightClickHandler

### Шаг 5.1: Проверка GameController
1. Откройте `Assets/_Project/Prefabs/GameController.prefab`
2. Убедитесь, что есть компонент **RightClickHandler**

### Шаг 5.2: Настройка Interactive Layers в RightClickHandler
В компоненте **RightClickHandler**:

1. Найдите массив **Interactive Layers**
2. Должно быть 2 элемента:
   
   **Element 0:**
   - **Type**: `Ground`
   - **Layer Mask**: выберите `Ground` (галочка на слое 8)
   
   **Element 1:**
   - **Type**: `Unit`
   - **Layer Mask**: выберите `Unit` (галочка на слое 9)

3. Если элементов нет:
   - Кликните на кружок рядом с **Size**, установите 2
   - Заполните как описано выше

### Шаг 5.3: Проверка камеры
- **Camera**: убедитесь, что назначена основная камера (обычно Main Camera)

### Шаг 5.4: Сохранение GameController
1. **Ctrl+S**
2. Выйдите из режима префаба

---

## Часть 6: Создание тестовой сцены для проверки боя

### Шаг 6.1: Подготовка сцены
1. Откройте `Assets/_Project/Scenes/SampleScene.unity` или создайте новую
2. Убедитесь, что есть:
   - **NavMesh Surface** (запечённый NavMesh)
   - **Ground** плоскость/террейн с слоем `Ground`
   - **GameController** (префаб из `_Project/Prefabs/GameController`)

### Шаг 6.2: Создание партии игрока
1. Перетащите `Unit.prefab` на сцену 3-4 раза
2. Для каждого юнита:
   - Установите позицию в разных местах
   - В компоненте **Unit** установите **Faction**: `User`
   - Убедитесь, что **UnitCombat** добавлен и настроен

### Шаг 6.3: Создание врагов
1. Перетащите `Unit.prefab` на сцену 2-3 раза
2. Для каждого врага:
   - Установите позицию подальше от партии
   - В компоненте **Unit** установите **Faction**: `Enemy`
   - В **UnitCombat** настройте статы (можно сделать сильнее)
   - Опционально: измените цвет selection visual (например, красный)

### Шаг 6.4: Настройка типа юнитов (melee vs ranged)
**Для melee юнита:**
- В **UnitCombat → Combat Stats**:
  - **Melee Range**: 1.5
  - **Ranged Max Range**: 2 (меньше чем meleeRange * 1.5 = 2.25)

**Для ranged юнита:**
- В **UnitCombat → Combat Stats**:
  - **Melee Range**: 1.5
  - **Ranged Min Range**: 5
  - **Ranged Max Range**: 15
  - **Projectile Prefab**: назначить `CombatProjectile`

### Шаг 6.5: Запекание NavMesh
1. Окно **Window → AI → Navigation**
2. Вкладка **Bake**
3. Настройте параметры:
   - **Agent Radius**: 0.5
   - **Agent Height**: 2
   - **Max Slope**: 45
4. Нажмите **Bake**

---

## Часть 7: Тестирование боевой системы

### Шаг 7.1: Запуск сцены
1. Нажмите **Play** в редакторе
2. Откройте консоль (**Window → General → Console**)

### Шаг 7.2: Проверка выделения
1. Кликните левой кнопкой на юните игрока → должен выделиться (цветной круг)
2. Зажмите ЛКМ и потяните → box selection
3. Shift+ЛКМ → добавление к выделению

### Шаг 7.3: Проверка движения
1. ПКМ на землю → юниты должны идти к точке
2. Должен появиться маркер движения (зелёный круг)

### Шаг 7.4: Проверка боя (Melee)
1. Выделите партию (ЛКМ или box)
2. ПКМ на враге (фракция Enemy)
3. Наблюдайте:
   - Юниты должны подойти к врагу на дистанцию meleeRange
   - Должна проиграться анимация атаки (PunchLeft)
   - Враг должен получить урон (проверьте логи в консоли)
   - В Editor: включите **Gizmos** (иконка вверху Scene view) для видимости радиусов

### Шаг 7.5: Проверка боя (Ranged)
1. Выделите ranged юнита
2. ПКМ на враге
3. Наблюдайте:
   - Юнит отходит на оптимальную дистанцию (между min и max range)
   - Должна проиграться анимация атаки
   - Должен лететь снаряд (CombatProjectile)
   - При попадании враг получает урон

### Шаг 7.6: Отладка через Gizmos
1. В Scene view включите **Gizmos** (иконка вверху)
2. Выберите юнита в иерархии
3. Вы увидите:
   - **Жёлтая сфера**: aggro range
   - **Красная сфера**: deaggro range
   - **Фиолетовая сфера** (melee): радиус ближней атаки
   - **Голубая/синяя сфера** (ranged): min/max range дальней атаки
   - **Зелёная линия**: текущая цель

---

## Часть 8: Настройка CI/CD Pipeline

### Шаг 8.1: Создание GitHub Actions workflow
1. Создайте файл `.github/workflows/build.yml` в корне проекта:

```yaml
name: Unity Build

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  build:
    name: Build for ${{ matrix.targetPlatform }}
    runs-on: ubuntu-latest
    strategy:
      fail-fast: false
      matrix:
        targetPlatform:
          - StandaloneWindows64
    
    steps:
    - uses: actions/checkout@v4
      with:
        lfs: true
    
    - uses: game-ci/unity-builder@v4
      env:
        UNITY_LICENSE: ${{ secrets.UNITY_LICENSE }}
        UNITY_EMAIL: ${{ secrets.UNITY_EMAIL }}
        UNITY_PASSWORD: ${{ secrets.UNITY_PASSWORD }}
      with:
        targetPlatform: ${{ matrix.targetPlatform }}
        buildName: CRPG_Build
        versioning: none
        
    - uses: actions/upload-artifact@v4
      with:
        name: Build-Windows
        path: build
```

### Шаг 8.2: Получение Unity License
1. Зарегистрируйтесь на https://license.unity3d.com/manual
2. Создайте Manual Activation License (.ulf файл)
3. Конвертируйте в base64: `base64 -i Unity_lic.ulf`
4. В GitHub репозитории: **Settings → Secrets and variables → Actions**
5. Добавьте секреты:
   - `UNITY_EMAIL`: ваш email
   - `UNITY_PASSWORD`: ваш пароль
   - `UNITY_LICENSE`: base64 строка лицензии

### Шаг 8.3: Альтернатива: локальный build script
Создайте файл `BuildScript.cs` в `Assets/_Project/Editor/`:

```csharp
using UnityEditor;
using UnityEngine;

public class BuildScript
{
    [MenuItem("Tools/CI/Build Windows")]
    public static void BuildWindows()
    {
        string[] scenes = { "Assets/_Project/Scenes/00_Bootstrap.unity" };
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = "Build/CRPG_Windows.exe",
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };
        
        BuildPipeline.BuildPlayer(buildPlayerOptions);
        Debug.Log("Build completed successfully!");
    }
}
```

Запуск: **Menu → Tools → CI/Build Windows**

---

## Часть 9: Чеклист готовности

### Код
- [x] `CombatStats.cs` - класс статов боя
- [x] `CombatProjectile.cs` - скрипт снаряда
- [x] `UnitCombat.cs` - основной компонент боя
- [x] `RightClickHandler.cs` - обновлён с HandleEnemyClick

### Префабы
- [ ] `CombatProjectile.prefab` создан и настроен
- [ ] `Unit.prefab` имеет компонент UnitCombat
- [ ] `GameController.prefab` имеет правильные Interactive Layers

### Animator
- [ ] Параметры `InCombat` (Bool) и `Attack` (Trigger) добавлены
- [ ] Состояния и переходы настроены
- [ ] Анимации `IdleCombat` и `PunchLeft` назначены

### Сцена
- [ ] NavMesh запечён
- [ ] Слои Ground/Unit назначены
- [ ] Тег Projectile создан
- [ ] Тестовые юниты (игроки и враги) расставлены

### CI/CD
- [ ] `.github/workflows/build.yml` создан
- [ ] Secrets в GitHub настроены
- [ ] Тестовый билд прошёл успешно

---

## Часть 10: Troubleshooting

### Проблема: Юниты не атакуют при клике на врага
**Решение:**
1. Проверьте, что у юнита есть компонент **UnitCombat**
2. Убедитесь, что у врага **Faction = Enemy**
3. Проверьте консоль на ошибки
4. Включите **Gizmos** и проверьте, видны ли радиусы атаки

### Проблема: Снаряды не летят
**Решение:**
1. Проверьте, что **Projectile Prefab** назначен в UnitCombat
2. Убедитесь, что у префаба снаряда есть компонент **CombatProjectile**
3. Проверьте, что **Attack Spawn Point** назначен
4. Убедитесь, что юнит считается ranged (RangedMaxRange > MeleeRange * 1.5)

### Проблема: Анимации не проигрываются
**Решение:**
1. Проверьте, что параметры `InCombat` и `Attack` существуют в Animator
2. Убедитесь, что переходы настроены правильно
3. Проверьте, что Animator назначен на префабе юнита
4. Включите **Info** в окне Animator для отладки

### Проблема: Ошибки компиляции
**Решение:**
1. Убедитесь, что все `.meta` файлы созданы
2. Проверьте namespace: `_Project.Scripts.Combat`
3. Перезапустите Unity Editor
4. **Assets → Reimport All**

---

## Дополнительные ресурсы

- Документация Unity: https://docs.unity3d.com/Manual/
- Навигация: https://docs.unity3d.com/Manual/nav-BasicConcept.html
- Animator: https://docs.unity3d.com/Manual/Animators.html
- GitHub Actions для Unity: https://game.ci/
