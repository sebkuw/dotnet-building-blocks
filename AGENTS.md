# Instrukcje dla agentów

## Zakres i pierwszeństwo

- Ten plik obowiązuje w całym repozytorium.
- Przed zmianą biblioteki przeczytaj także jej `src/<library>/AGENTS.md`, `README.md`, `CHANGELOG.md`, plik projektu oraz odpowiadający projekt w `tests/`.
- Instrukcje położone bliżej zmienianego pliku uzupełniają ten dokument i mają pierwszeństwo w razie konfliktu.
- Dla zmian przekrojowych użyj skilla `develop-dotnet-building-blocks`; dla pojedynczej biblioteki użyj również odpowiadającego jej skilla `develop-netdevs-*`.
- Nie modyfikuj wygenerowanych katalogów `bin/`, `obj/`, `artifacts/`, `.vs/` ani plików paczek NuGet.
- Zachowuj niezwiązane i niezacommitowane zmiany użytkownika.

## Platforma i zależności

- Używaj wyłącznie .NET 10 i C# 14 zgodnie z `global.json` oraz wspólnymi plikami `Directory.*.props`.
- Nie obniżaj poziomu kontroli `Nullable`, analizatorów, traktowania ostrzeżeń jako błędów ani pozostałych bramek jakości, aby obejść problem.
- Preferuj BCL, ASP.NET Core i istniejące zależności. Nową zależność dodawaj tylko wtedy, gdy istotnie upraszcza poprawne rozwiązanie.
- Dopuszczaj wyłącznie bezpłatne zależności open source, również do użytku komercyjnego, na licencjach MIT, Apache-2.0, BSD-2-Clause, BSD-3-Clause lub ISC. Zasada obejmuje zależności przechodnie.
- Przed dodaniem lub aktualizacją paczki sprawdź licencję, zależności przechodnie, podatności, aktywność projektu i zgodność z .NET 10. Wyjątek licencyjny wymaga jawnej zgody użytkownika.
- Utrzymuj wersje paczek centralnie w `Directory.Packages.props` i aktualizuj pliki `packages.lock.json` przez przywrócenie zależności.
- Nie dodawaj płatnych usług, komponentów wymagających komercyjnej licencji ani zależności o niejasnych warunkach.

## Projektowanie bibliotek

- Projektuj małe, spójne paczki NuGet, które można wykorzystać niezależnie w wielu aplikacjach.
- Nie wprowadzaj zależności od konkretnej aplikacji, domeny, bazy danych, dostawcy tożsamości, systemu logowania ani endpointu.
- Utrzymuj kierunek zależności od abstrakcji do implementacji. Nie twórz cykli między paczkami.
- Ograniczaj publiczne API do elementów potrzebnych konsumentom. Szczegóły implementacyjne pozostawiaj `internal` lub `private`.
- Traktuj publiczne typy, sygnatury, zachowanie, format JSON, wartości domyślne i rejestracje DI jako kontrakt wersjonowany zgodnie z SemVer.
- Zmiana niezgodna wstecznie wymaga uzasadnienia, instrukcji migracji, wpisu w dzienniku zmian i podniesienia wersji głównej. Gdy to możliwe, najpierw oznacz API jako przestarzałe.
- Stosuj asynchroniczne API z `CancellationToken`, gdy operacja może czekać na I/O. Nie ukrywaj synchronicznego blokowania.
- Wstrzykuj czas, użytkownika, środowisko i inne źródła niedeterministyczne. Unikaj globalnego stanu i ukrytych efektów ubocznych.
- Waliduj argumenty na granicy publicznego API i zwracaj przewidywalne, udokumentowane błędy.
- Nie ujawniaj sekretów, danych osobowych, zapytań, ścieżek systemowych ani śladu stosu w odpowiedziach dla klienta.

## Zgodność z angular-shared-components

- Traktuj `C:\A_REPOS\angular-shared-components` jako referencyjnego konsumenta kontraktów HTTP.
- Przed zmianą filtrowania, sortowania, paginacji, odpowiedzi błędów lub identyfikatora korelacji przeczytaj `docs/angular-shared-components-contract.md` oraz aktualne modele w `projects/shared-ui-list`.
- Zachowuj zgodność z `@netdevs/shared-ui-list`: numeryczne wartości `FilterOperation`, 1-based `PageNumber`, `PageSize` do 100, sortowanie `Property asc|desc` i jawne nazwy JSON opisane w kontrakcie.
- Akceptuj starsze formaty kontraktu, jeśli ich usunięcie nie zostało zatwierdzone jako zmiana niezgodna wstecznie.
- Każdą zmianę kontraktu Angular/.NET zabezpiecz testem serializacji lub deserializacji oraz przykładem w dokumentacji.
- Frontendowa kontrola permissions/claims jest wyłącznie warstwą UX; autoryzację zawsze egzekwuje backend.

## Wymagane testy

- Każda nowa funkcja i każda zmiana zachowania musi mieć test scenariusza poprawnego, brzegowego, niepoprawnych danych i anulowania, jeśli dotyczy.
- Naprawiony błąd musi otrzymać test regresyjny, który nie przechodził przed poprawką.
- Dodawaj testy jednostkowe dla czystej logiki oraz testy integracyjne dla DI, oprogramowania pośredniczącego, EF Core, serializacji i dostawców LINQ.
- Dodawaj testy kontraktowe dla publicznych typów, JSON i współpracy z `angular-shared-components`.
- Dla pakietów sprawdzaj kompilację, zawartość `.nupkg`, README, dziennik zmian i możliwość użycia publicznego API bez importów wewnętrznych lub zależności projektowych.
- Dla zmian architektury sprawdzaj kierunek zależności, brak cykli i brak wycieku infrastruktury do paczek abstractions.
- Mutation testing stosuj dla logiki o istotnym ryzyku: filtrowania, autoryzacji, mapowania wyjątków, audytu i obliczeń. Uzasadnij pominięcie, jeśli infrastruktura nie jest dostępna.
- Utrzymuj co najmniej 90% pokrycia linii i 80% gałęzi dla każdej biblioteki. Nie pisz testów wyłącznie pod procent i nie usuwaj wartościowych asercji.
- Testy mają być deterministyczne, niezależne od kolejności, strefy czasowej, internetu i współdzielonego stanu.

## Dokumentacja i dziennik zmian

- Każda biblioteka musi mieć aktualny `README.md` opisujący cel, instalację, publiczne API, konfigurację, przykłady, ograniczenia, testowanie i kompatybilność.
- Aktualizuj dokumentację w tym samym zadaniu co kod. Przykłady muszą używać wyłącznie publicznego API.
- Utrzymuj główny `CHANGELOG.md` dla zmian przekrojowych oraz osobny `src/<library>/CHANGELOG.md` dla każdej paczki.
- Stosuj format Keep a Changelog i zasady Semantic Versioning. Każde zadanie zmieniające repozytorium aktualizuje co najmniej jeden właściwy dziennik zmian.
- Zmianę biblioteki zapisuj w sekcji `Unreleased` jej dziennika zmian pod właściwym nagłówkiem: `Added`, `Changed`, `Deprecated`, `Removed`, `Fixed` lub `Security`. Zmiana przekrojowa aktualizuje również główny dziennik zmian.
- Dziennik zmian aktualizuj na poziomie gotowej zmiany lub PR, nie każdego tymczasowego zatwierdzenia.
- Linkuj dziennik zmian z README biblioteki i pakuj oba dokumenty do odpowiedniej paczki NuGet.

## Weryfikacja i zakończenie pracy

- Podczas iteracji uruchamiaj najwęższy właściwy projekt testowy.
- Przed zakończeniem zmiany biblioteki uruchom jej testy, kompilację i pakowanie. Dla zmiany przekrojowej uruchom pełny zestaw:

```powershell
dotnet restore NetDevs.BuildingBlocks.slnx
./eng/verify-package-licenses.ps1
dotnet format NetDevs.BuildingBlocks.slnx --verify-no-changes --no-restore
dotnet build NetDevs.BuildingBlocks.slnx --configuration Release --no-restore
dotnet test NetDevs.BuildingBlocks.slnx --configuration Release --no-build --settings coverlet.runsettings --collect:"XPlat Code Coverage" --results-directory artifacts/TestResults
./eng/verify-coverage.ps1
dotnet pack NetDevs.BuildingBlocks.slnx --configuration Release --no-build --output artifacts/packages
./eng/verify-packages.ps1
```

- Sprawdź `git diff`, publiczne API, dokumentację, dzienniki zmian, pliki blokad zależności i przypadkowe pliki wygenerowane.
- Nie deklaruj testu jako wykonanego, jeśli go nie uruchomiono. Wskaż dokładnie niewykonaną walidację i przyczynę.

## Przegląd kodu

- Zgłaszaj jako błąd zmianę publicznego zachowania bez testów, dokumentacji i changelogu.
- Zgłaszaj nieudokumentowane zmiany niezgodne wstecznie, niezgodne kontrakty JSON, utratę identyfikatora korelacji oraz rozbieżność z `angular-shared-components`.
- Zgłaszaj zależności płatne, podatne, niezgodne licencyjnie lub dodane bez wyraźnej potrzeby.
- Zgłaszaj luki autoryzacji, wycieki informacji, nieobsłużone anulowanie, globalny stan i niedeterministyczne testy.
