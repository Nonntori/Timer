# Таймер выключения (Shutdown Timer)

Современное Windows-приложение для автоматического выключения, перезагрузки или выполнения других действий по таймеру.

## Особенности

- **Современный дизайн** в стиле Windows 11 Fluent Design
- **Тёмная тема** с акцентными цветами
- **Круговой индикатор** оставшегося времени
- **Быстрый выбор** времени (15 мин, 30 мин, 1 час, 2 часа)
- **Настраиваемое действие**: выключение, перезагрузка, выход из системы, спящий режим
- **Предупреждение** перед выполнением действия
- **Уведомления Windows**
- **Автозапуск** с Windows
- **Работа в трее**

## Требования

- Windows 10/11
- .NET 8.0 SDK

## Сборка

```bash
cd ShutdownTimer
dotnet restore
dotnet build
```

## Запуск

```bash
dotnet run
```

## Публикация Release

```bash
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true
```

После публикации исполняемый файл будет находиться в:
`bin/Release/net8.0-windows/win-x64/publish/ShutdownTimer.exe`

## Структура проекта

```
ShutdownTimer/
├── Models/           # Модели данных
├── ViewModels/       # ViewModel для MVVM
├── Views/            # XAML представления
├── Services/         # Бизнес-логика и сервисы
├── Converters/       # Конвертеры для binding
├── Resources/        # Ресурсы
├── Assets/           # Иконки и изображения
└── App.xaml          # Главный файл приложения
```

## Используемые технологии

- WPF (.NET 8)
- CommunityToolkit.Mvvm
- Microsoft.Extensions.DependencyInjection
- Newtonsoft.Json

## Лицензия

MIT License
