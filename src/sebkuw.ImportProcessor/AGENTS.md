# Instrukcje dla sebkuw.ImportProcessor

- Zachowuj pakiet niezależny od domeny, ORM, kontenera DI i konkretnego formatu trwałego magazynu.
- Odczyt CSV musi pozostać strumieniowy i poprawnie obsługiwać cudzysłowy, separatory oraz wielowierszowe pola.
- Publiczne operacje I/O muszą przyjmować `CancellationToken`.
- Mapowanie, konwersja, walidacja, zapis i idempotencja są punktami rozszerzeń konsumenta; pakiet nie wykonuje domenowego upsertu.
- Utrzymuj kod w katalogach `Abstractions`, `Configuration`, `Internal`, `Mapping`, `Models` i `Processing`, zachowując publiczne typy w namespace `sebkuw.ImportProcessor`.
- Testuj nagłówki, cytowanie, konwersje, walidację, preview, zapis, duplikaty, anulowanie i niepoprawne argumenty.
