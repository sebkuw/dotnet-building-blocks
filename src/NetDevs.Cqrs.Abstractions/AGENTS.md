# Instrukcje dla NetDevs.Cqrs.Abstractions

- Utrzymuj paczkę bez zależności produkcyjnych i niezależną od konkretnego mediatora oraz kontenera DI.
- Dodawaj wyłącznie kontrakty komend, zapytań i procedur obsługi potrzebne wielu implementacjom.
- Zachowuj silne typowanie odpowiedzi, kontrawariancję wejścia i obowiązkowy `CancellationToken` w procedurach obsługi.
- Nie dodawaj mechanizmu wysyłania komunikatów, potoku przetwarzania, walidacji ani rejestracji usług do warstwy abstrakcji.
- Testuj sygnatury procedur obsługi, typ odpowiedzi i propagację anulowania.
- Użyj skilla `develop-netdevs-cqrs-abstractions` i zweryfikuj `tests/NetDevs.Cqrs.Abstractions.Tests`.
