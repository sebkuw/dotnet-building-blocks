---
name: develop-netdevs-cqrs
description: Utrzymuj wykrywanie procedur obsługi i ich rejestrację w mechanizmie wstrzykiwania zależności Microsoft w bibliotece NetDevs.Cqrs. Używaj przy zmianach CqrsRegistrationOptions, metod AddCqrs, skanowania zestawów .NET, czasów życia, walidacji rejestracji, zależności, testów, dokumentacji lub informacji o wydaniu paczki integracyjnej CQRS.
---

# Rozwijanie NetDevs.Cqrs

## Przebieg pracy

1. Przeczytaj główny i lokalny `AGENTS.md`, dokumentację paczki, abstrakcje CQRS, kod rejestracji oraz `tests/NetDevs.Cqrs.Tests`.
2. Ogranicz odpowiedzialność biblioteki do jawnego wyboru zestawów .NET i rejestracji obsługiwanych kontraktów w kontenerze DI.
3. Zachowaj zakresowy (`Scoped`) czas życia i rozwiązywanie usług przez zaimplementowane publiczne interfejsy, chyba że zatwierdzono wersjonowaną zmianę API.
4. Zwróć czytelny błąd, gdy nie skonfigurowano żadnego zestawu. Unikaj niejawnego skanowania całej aplikacji.
5. Testuj komendy bez wyniku i z wynikiem, zapytania, zakresowy czas życia, wiele zestawów, niepubliczne procedury obsługi, duplikaty, pustą konfigurację oraz zachowanie kontraktów anulowania.
6. Ponownie uruchom testy abstrakcji po każdej zmianie reguł skanowania lub obsługiwanych kontraktów.
7. Zaktualizuj README i sekcję `Unreleased` dziennika zmian, a następnie uruchom testy CQRS, kompilację Release, pakowanie i kontrolę paczki.
