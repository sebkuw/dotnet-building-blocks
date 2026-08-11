---
name: develop-dotnet-building-blocks
description: Utrzymuj kompletne repozytorium bibliotek NetDevs dla .NET 10 obejmujące wiele paczek. Używaj przy przekrojowych zmianach architektury, zależności, kompilacji, CI, wydań, dokumentacji, dzienników zmian, zasad testowania lub kontraktu z Angular, które dotyczą więcej niż jednej biblioteki NetDevs.
---

# Rozwijanie bibliotek .NET Building Blocks

## Przebieg pracy

1. Przeczytaj główne pliki `AGENTS.md`, `README.md`, `CHANGELOG.md`, `Directory.Build.props`, `Directory.Packages.props`, `global.json`, rozwiązanie oraz bieżące różnice Git.
2. Określ wszystkie objęte zmianą paczki. Dla każdej przeczytaj lokalne pliki `AGENTS.md`, `README.md`, `CHANGELOG.md`, plik projektu, publiczny kod źródłowy, testy i właściwego skilla biblioteki.
3. Zachowaj istniejące, niezwiązane zmiany. Przed edycją określ wpływ na publiczne API, kontrakt JSON, zależności, wersjonowanie i migrację.
4. Utrzymuj wszystkie projekty na .NET 10. Dodawaj tylko centralnie wersjonowane zależności dozwolone przez politykę licencyjną repozytorium.
5. Wprowadź najmniejszą reużywalną zmianę, bez założeń właściwych konkretnej aplikacji i bez cykli zależności.
6. Dodaj odpowiednie testy jednostkowe, integracyjne, kontraktowe, regresyjne, architektury i paczek. Utrzymuj dla każdej biblioteki co najmniej 90% pokrycia linii i 80% gałęzi.
7. Zaktualizuj README oraz sekcję `Unreleased` dziennika zmian każdej objętej zmianą paczki. Dla zmian przekrojowych zaktualizuj również główną dokumentację i dziennik zmian.
8. Uruchom opisane w głównym `AGENTS.md`: przywrócenie zależności, kontrolę licencji i formatowania, kompilację Release, testy z pokryciem, pakowanie oraz kontrolę zawartości paczek.
9. Sprawdź końcowe różnice pod kątem zgodności publicznego API, aktualizacji plików blokad zależności, plików wygenerowanych i przypadkowych zmian.

## Kontrakt między repozytoriami

Przed zmianą kontraktów zapytań lub błędów HTTP przeczytaj `docs/angular-shared-components-contract.md` oraz aktualne modele w `C:\A_REPOS\angular-shared-components\projects\shared-ui-list`. Preferuj przetwarzanie zgodne wstecznie i zabezpieczaj każdy kontrakt dokładnymi testami serializacji.

## Warunek ukończenia

Nie kończ pracy na samym kodzie. Uznaj zmianę za ukończoną dopiero wtedy, gdy implementacja, właściwe testy, dokumentacja, dziennik zmian, kontrole zależności i proporcjonalny zestaw weryfikacji są ze sobą spójne.
