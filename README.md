# VaciniaBot

Простой бот для Discord, написанный с использованием библиотеки [DSharpPlus](https://github.com/DSharpPlus/DSharpPlus).
Бот создавался для проекта Vacinia на основе функционала плагина [DiscordSRV](https://github.com/DiscordSRV/DiscordSRV) и [OpenLogin](https://github.com/nickuc/OpeNLogin)

## Функции

- /clear [значение] - очистка определенного кол-во сообщений (не больше 1000 за раз)
- /say [текст] - отправка любого текста от имени бота
- /ticket - отправляет в канал сообщение типа DropDown с 2 пунктами, создающий канал по выбору:
    - Whitelist - канал для принятия заявок от игроков
    - Report - канал для принятия жалоб от игроков
- /players - выводит полный список игроков (используйте удаленную базу данных, на данный момент бот не поддерживает использование RCON)


## Конфигурация

Конфигурация в файле config.json:

```bash
  {
    "token": "token", //токен бота Discord
    "prefix": "!", //префикс для команд
    "adminRoles": [ "0000000000000000000", "0000000000000000000" ], //Id ролей администрации в Discord
    "logChannelId": "0000000000000000000", //Канал с оповещениями о принятии заявок в Whitelist
    "consoleChannelId": "0000000000000000000", //Канал-консоль, все сообщения что попадают сюда вводятся в консоль вашего сервера (Требуется DiscordSRV)
    "mysql": {
        "server": "server", //Endpoint или хост вашей БД
        "port": 3306, //Порт по умолчанию 3306
        "database": "database", //Имя БД
        "user": "user", //Имя для входа в БД
        "password": "password", //Пароль для входа в БД
        "table": "name_table", //Название таблицы с колонной, содержащий никнеймы ваших игроков
        "column": "name_column" //Название колонны с именами ваших игроков
    }
}
```
    
## License

[GPLv3](https://opensource.org/license/lgpl-3-0)

[![GPLv3 License](https://img.shields.io/badge/License-GPL%20v3-yellow.svg)](https://opensource.org/license/lgpl-3-0)



## Authors

- [@clonernotfound](https://www.github.com/clonernotfound)


## Support

Для поддержки используйте clonernotfound@gmail.com или [Issues](https://github.com/ClonerNotFound/VaciniaBot/issues).
