---
name: develop-netdevs-queryable-processor
description: Utrzymuj filtrowanie, sortowanie, paginację, projekcję, wykonywanie zapytań EF Core i zgodność JSON z Angular shared-ui-list w NetDevs.QueryableProcessor. Używaj przy zmianach modeli żądań lub odpowiedzi, wartości FilterOperation, budowania wyrażeń, przetwarzania sortowania, zasad paginacji, solverów, testów, dokumentacji albo informacji o wydaniu.
---

# Rozwijanie procesora zapytań

## Przebieg pracy

1. Przeczytaj główny i lokalny `AGENTS.md`, dokumentację paczki, `docs/angular-shared-components-contract.md`, cały kod procesora zapytań, jego testy oraz aktualne modele paginacji `shared-ui-list`.
2. Zachowaj numeryczne wartości `FilterOperation` 0-10, numerowanie stron od 1, maksymalny rozmiar strony 100 i jawne nazwy JSON zapisane w PascalCase.
3. Akceptuj format sortowania Angular `Property asc|desc` i starszy `asc_Property|desc_Property`. Rozwiązuj zagnieżdżone ścieżki właściwości bez uwzględniania wielkości liter.
4. Waliduj niezaufane ścieżki właściwości, operacje, wartości i paginację bez wywoływania dowolnych metod wybranych przez klienta.
5. Buduj wyrażenia możliwe do przetłumaczenia przez dostawcę zapytań i odkładaj materializację do zakończenia filtrowania, sortowania, zliczania i paginacji.
6. Testuj każdą operację, wartości `null` i niepoprawne, typy wyliczeniowe, identyfikatory GUID, daty, liczby, tablice JSON, zagnieżdżone ścieżki, oba formaty sortowania, wartości graniczne, serializację i wykonanie przez EF Core.
7. Zaktualizuj README, dokumentację kontraktu i sekcję `Unreleased` dziennika zmian, a następnie uruchom testy zapytań, kompilację Release, kontrolę pokrycia, pakowanie i kontrolę paczki.
