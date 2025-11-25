rmc-bioscan-ares-announcement = [color=white][font size=16][bold]ARES v3.2 — состояние биосканирования[/bold][/font][/color][color=red][font size=14][bold]
    {$message}[/bold][/font][/color]

rmc-bioscan-ares = Биоскан завершён.

  Сенсоры показывают { $shipUncontained ->
    [0] отсутствие
    *[other] {$shipUncontained}
  } неизвестных форм жизни { $shipUncontained ->
    [0] сигнатур
    [1] сигнатура
    *[other] сигнатур
  } на борту корабля{ $shipLocation ->
    [none] {""}
    *[other], включая одну в {$shipLocation},
  } и { $onPlanet ->
    [0] нет
    *[other] примерно {$onPlanet}
  } { $onPlanet ->
    [0] сигнатур
    [1] сигнатура
    *[other] сигнатур
  } обнаружено в других местах{ $planetLocation ->
    [none].
    *[other], включая одну в {$planetLocation}
  }

rmc-bioscan-xeno-announcement = [color=#318850][font size=14][bold]Матерь-Королева проникает в твой разум с далёких миров.
  {$message}[/bold][/font][/color]

rmc-bioscan-xeno = Моим детям и их Королеве: Я чувствую { $onShip ->
  [0] отсутствие носителей
  [1] приблизительно одного носителя
  *[other] примерно {$onShip} носителей
} в металлическом улье{ $shipLocation ->
  [none] {""}
  *[other], включая одного в {$shipLocation},
} и {$onPlanet ->
  [0] никого
  *[other] {$onPlanet}
} разбросанных в иных местах{$planetLocation ->
  [none].
  *[other], включая одного в {$planetLocation}
}
