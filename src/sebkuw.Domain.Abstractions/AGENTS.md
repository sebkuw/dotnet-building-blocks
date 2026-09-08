# Instrukcje dla sebkuw.Domain.Abstractions

- Utrzymuj zależności produkcyjne na poziomie zera; ta paczka jest najniższą warstwą domenową.
- Umieszczaj tu wyłącznie ogólne kontrakty encji i audytu, bez EF Core, ASP.NET Core, DI ani logowania.
- Nie narzucaj domenie generatora identyfikatorów, zegara, bieżącego użytkownika ani strategii persystencji.
- Zachowuj proste typy audytu zgodne z `sebkuw.EntityFrameworkCore.Auditing`.
- Testuj implementowane interfejsy, mutowalność kontraktu, typ identyfikatora, wartości domyślne i metadane audytu.
- Użyj skilla `develop-sebkuw-domain-abstractions` i zweryfikuj `tests/sebkuw.Domain.Abstractions.Tests`.
