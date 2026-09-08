# Instrukcje dla sebkuw.EntityFrameworkCore.Auditing

- Utrzymuj audyt jako interceptor EF Core oparty na kontraktach z `sebkuw.Domain.Abstractions`.
- Pobieraj czas i użytkownika wyłącznie przez wstrzykiwane abstrakcje; wszystkie czasy zapisuj w UTC.
- Nie nadpisuj metadanych utworzenia podczas aktualizacji i nie ustawiaj metadanych aktualizacji dla nowej encji bez jawnej zmiany kontraktu.
- Obsługuj synchroniczne i asynchroniczne `SaveChanges` spójnie oraz propaguj `CancellationToken`.
- Testuj stany `Added`, `Modified`, `Deleted` i `Unchanged`, ochronę pól utworzenia, wiele encji, brak użytkownika oraz anulowanie.
- Użyj skilla `develop-sebkuw-efcore-auditing` i zweryfikuj `tests/sebkuw.EntityFrameworkCore.Auditing.Tests`.
