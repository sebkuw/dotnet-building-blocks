---
name: develop-sebkuw-cqrs-abstractions
description: Utrzymuj pozbawione zależności kontrakty komend, zapytań i procedur obsługi CQRS w sebkuw.Cqrs.Abstractions. Używaj przy zmianach ICommand, IQuery, interfejsów procedur obsługi, generycznego typowania odpowiedzi, semantyki anulowania, publicznego API paczki, testów, dokumentacji lub informacji o wydaniu.
---

# Rozwijanie abstrakcji CQRS

## Przebieg pracy

1. Przeczytaj główny i lokalny `AGENTS.md`, dokumentację paczki, wszystkie kontrakty CQRS oraz `tests/sebkuw.Cqrs.Abstractions.Tests`.
2. Utrzymuj niezależność paczki od bibliotek mediatora, wstrzykiwania zależności, mechanizmów wysyłania komunikatów, walidacji i implementacji potoków.
3. Zachowaj silne typowanie odpowiedzi, wariancję wejścia i `CancellationToken` w kontraktach procedur obsługi.
4. Traktuj nowe interfejsy bazowe, ograniczenia, przeciążenia, wariancję i typy zwracane jako decyzje dotyczące zgodności publicznego API.
5. Dodaj implementacje sprawdzane podczas kompilacji oraz testy zachowania obejmujące komendy bez odpowiedzi i z odpowiedzią, zapytania, typowane wyniki i propagację anulowania.
6. Po zmianach kontraktów sprawdź zgodność rejestracji w `sebkuw.Cqrs`.
7. Zaktualizuj README i sekcję `Unreleased` dziennika zmian, a przed pakowaniem uruchom testy abstrakcji i rejestracji CQRS.
