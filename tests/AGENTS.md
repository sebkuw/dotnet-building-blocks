# Instrukcje dla testów

- Stosuj zasady testowania i jakości z głównego `AGENTS.md`.
- Odwzorowuj bibliotekę `src/NetDevs.X` w projekcie `tests/NetDevs.X.Tests`.
- Nazywaj testy według obserwowalnego zachowania. Stosuj układ arrange-act-assert bez komentarzy zastępujących czytelny kod.
- Testuj przez publiczne API. Mechanizmu refleksji używaj tylko do jawnych testów kontraktu lub architektury.
- Nie współdziel mutowalnego stanu między testami i nie polegaj na kolejności wykonania.
- Używaj EF Core InMemory tylko do zachowań niezależnych od semantyki relacyjnej. Dla zachowań zależnych od dostawcy dodaj test z właściwym bezpłatnym dostawcą.
- Dla oprogramowania pośredniczącego sprawdzaj status, typ zawartości, nagłówki, dokładny kontrakt JSON i zachowanie całego potoku.
- Dla kodu asynchronicznego przekazuj i sprawdzaj `CancellationToken`; nie używaj opóźnień czasowych jako synchronizacji.
- Każda nowa biblioteka produkcyjna wymaga odpowiadającego projektu testowego dodanego do rozwiązania.
