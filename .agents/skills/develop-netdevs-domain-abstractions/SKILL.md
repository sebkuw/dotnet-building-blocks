---
name: develop-netdevs-domain-abstractions
description: Utrzymuj kontrakty encji i audytu w NetDevs.Domain.Abstractions. Używaj przy zmianach IEntity, Entity, GuidEntity, bazowych typów encji audytowalnych, interfejsów audytu utworzenia lub aktualizacji, API paczki, testów, dokumentacji albo informacji o wydaniu pozbawionej zależności paczki domenowej.
---

# Rozwijanie abstrakcji domenowych

## Przebieg pracy

1. Przeczytaj główny i lokalny `AGENTS.md`, README i dziennik zmian paczki, wszystkie pliki źródłowe oraz `tests/NetDevs.Domain.Abstractions.Tests`.
2. Utrzymuj paczkę bez zależności produkcyjnych i bez odpowiedzialności infrastrukturalnych.
3. Traktuj kształt identyfikatora, ograniczenia generyczne, mutowalność, typy właściwości audytu, obsługę wartości `null` i dziedziczenie jako publiczne API.
4. Nie osadzaj w kontraktach domenowych zasad generowania identyfikatorów, trwałego zapisu, pobierania bieżącego użytkownika ani czasu.
5. Dodaj dla każdego zmienionego typu testy kontraktowe wykonywane podczas kompilacji i uruchomienia, obejmujące wartości domyślne, implementację interfejsów, identyfikatory generyczne i metadane audytu.
6. Przy zmianie kontraktów audytu sprawdź zgodność z `NetDevs.EntityFrameworkCore.Auditing`.
7. Zaktualizuj `README.md` i `CHANGELOG.md`, a następnie uruchom projekt testów domenowych, kompilację Release i kontrolę paczki.
