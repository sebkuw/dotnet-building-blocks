---
name: develop-netdevs-efcore-auditing
description: Utrzymuj usługi NetDevs.EntityFrameworkCore.Auditing i zachowanie interceptora SaveChanges. Używaj przy zmianach dostawców audytu, abstrakcji bieżącego użytkownika lub czasu, rejestracji DI, obsługi stanów encji, synchronicznego lub asynchronicznego SaveChanges, testów, dokumentacji albo informacji o wydaniu.
---

# Rozwijanie audytu EF Core

## Przebieg pracy

1. Przeczytaj główny i lokalny `AGENTS.md`, dokumentację paczki, domenowe kontrakty audytu, kod interceptora, rozszerzenia rejestracji i testy audytu.
2. Pobieraj użytkownika i czas UTC wyłącznie przez wstrzykiwane abstrakcje. Utrzymuj deterministyczność testów.
3. Podczas aktualizacji zachowaj metadane utworzenia. Przy pierwszym zapisie nie ustawiaj metadanych aktualizacji, chyba że zmienił się udokumentowany kontrakt.
4. Utrzymuj równoważne zachowanie synchronicznego i asynchronicznego przechwytywania oraz propaguj anulowanie.
5. Testuj stany `Added`, `Modified`, `Deleted` i `Unchanged`, ochronę pól utworzenia, wiele encji, zastępczych dostawców, brak wartości użytkownika i anulowanie operacji asynchronicznych.
6. Sprawdź zgodność ze wszystkimi audytowalnymi typami w `NetDevs.Domain.Abstractions`.
7. Zaktualizuj README i sekcję `Unreleased` dziennika zmian, a następnie uruchom testy audytu i domeny, kompilację Release, pakowanie oraz kontrolę paczki.
