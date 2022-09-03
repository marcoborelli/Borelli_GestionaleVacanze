@echo off

rem durata retention
set retention=7

rem calcolo modulo giorno
set /a giorno=%date:~0,2%
set /a modulo=giorno%%retention

echo ---------------------------------------
echo retentio: %retention%
echo giorno data: %giorno%
echo modulo: %modulo%
echo ---------------------------------------

rem salvataggio archivi

"\program files\7-zip\7z" a backup\backup.%modulo%.zip *.ristorante >nul

start borelli_gestionalevacanze
