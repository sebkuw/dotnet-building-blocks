# Instrukcje dla sebkuw.Cqrs

- Ogranicz odpowiedzialność do integracji kontraktów CQRS z `Microsoft.Extensions.DependencyInjection`.
- Skanuj tylko jawnie wskazane zestawy .NET. Brak konfiguracji ma kończyć się czytelnym błędem.
- Rejestruj procedury obsługi przez ich publiczne kontrakty i zachowuj udokumentowany zakresowy (`Scoped`) czas życia.
- Nie dodawaj mechanizmu wysyłania komunikatów, zachowań potoku ani zależności od konkretnej aplikacji bez wyodrębnienia i osobnej decyzji dotyczącej API.
- Testuj wszystkie obsługiwane warianty procedur obsługi, czasy życia, wiele zestawów .NET, typy niepubliczne, duplikaty i błędną konfigurację.
- Użyj skilla `develop-sebkuw-cqrs` i zweryfikuj `tests/sebkuw.Cqrs.Tests`.
