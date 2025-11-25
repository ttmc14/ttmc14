# Requisition Computer
requisition-paperwork-receiver-name = Отдел логистики
requisition-paperwork-reward-message = Подтверждение получено! ${ $amount } переведено из резервного бюджета

# Requisition Invoice
requisition-paper-print-name = Счёт { $name }
requisition-paper-print-manifest = [head=2]
    { $containerName }[/head][bold]{ $content }[/bold][head=2]
    ВЕС { $weight } LBS
    ЛОТ { $lot }
    S/N { $serialNumber }[/head]
requisition-paper-print-content = - { $count } { $item }

# Supply Drop Console
ui-supply-drop-consle-name = Консоль доставки припасов
ui-supply-drop-console-name-bolded = [bold]ДОСТАВКА ПРИПАСОВ[/bold]
ui-supply-drop-console-longitude = Долгота:
ui-supply-drop-console-latitude = Широта:
ui-supply-drop-pad-status = [bold]Статус площадки[/bold]
ui-supply-drop-console-update = Обновить
ui-supply-drop-console-ready = Готово к запуску!
ui-supply-drop-console-launch = ЗАПУСТИТЬ ДОСТАВКУ
ui-supply-drop-console-cooldown = До следующего запуска { $time } секунд
ui-supply-drop-crate-status =
    { $hasCrate ->
        [true] Статус площадки: контейнер загружен.
       *[false] Контейнер не загружен.
    }
