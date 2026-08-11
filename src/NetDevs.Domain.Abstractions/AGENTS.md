# Instrukcje dla NetDevs.Domain.Abstractions

- Utrzymuj zależności produkcyjne na poziomie zera; ta paczka jest najniższą warstwą domenową.
- Umieszczaj tu wyłącznie ogólne kontrakty encji i audytu, bez EF Core, ASP.NET Core, DI ani logowania.
- Nie narzucaj domenie generatora identyfikatorów, zegara, bieżącego użytkownika ani strategii persystencji.
- Zachowuj proste typy audytu zgodne z `NetDevs.EntityFrameworkCore.Auditing`.
- Testuj implementowane interfejsy, mutowalność kontraktu, typ identyfikatora, wartości domyślne i metadane audytu.
- Użyj skilla `develop-netdevs-domain-abstractions` i zweryfikuj `tests/NetDevs.Domain.Abstractions.Tests`.
