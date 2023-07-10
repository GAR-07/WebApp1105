Web приложение, позволяющее осуществлять вход в аккаунт одним из двух выбранных способов авторизации и аутентификации.
Имеется возможность добавления, просмотра, редактирования и удаления книг, фото и видео.

Требуется установить:
Visual Studio 2022 (https://visualstudio.microsoft.com/)
Node.js 18.16.0 (x64) (https://nodejs.org/)
Приложение ffmpeg.exe (https://ffmpeg.org/)
Использовался MS SQL Server v19, но должен подойти и другой SQL сервер.

Произвести настройку путей до базы данных в appsettings.json (в StorageController для ffmpeg).
Загруженные файлы хранятся в Resources папке API части проекта.

API часть проекта открывается через Visual Studio и запускается.
Все взаимодействия происходят через UI, запускается через консоль командами, находясь по адрессу папки WebApp1105.UI:
npm i
ng serve --o

Используются:
ASP.NET 6
Angular 16.0.2
jquery 3.7.0
bootstrap 5.3.0-alpha1.bundle
Иконка от fontawesome (https://fontawesome.com/)

Миграции для базы данных созданы, но их можно удалить и создать заново командой в консоли пакетов NuGet: add-migration [name]
И требуется в любом случае обновить базу данных командой в той же консоли: update-database

Проект на GitHub: https://github.com/GAR-07/WebApp1105
