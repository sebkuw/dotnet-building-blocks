# Instrukcje dla sebkuw.QueryableProcessor

- Utrzymuj zgodność kontraktu z `@netdevs/shared-ui-list` opisaną w `docs/angular-shared-components-contract.md`.
- Akceptuj format sortowania Angular `Property asc|desc` oraz starszy `asc_Property|desc_Property`; ścieżki właściwości rozpoznawaj bez względu na wielkość liter.
- Zachowuj numeryczne wartości `FilterOperation` 0-10. Ich zmiana jest niezgodną wstecznie zmianą kontraktu JSON.
- Waliduj ścieżki, operację, typ wartości, `PageNumber >= 1` i `PageSize` od 1 do 100. Nie wykonuj dowolnych metod wskazanych przez klienta.
- Buduj wyrażenia możliwe do przetłumaczenia przez EF Core; nie wymuszaj materializacji przed filtrowaniem, sortowaniem, zliczaniem i paginacją.
- Zachowuj jawne nazwy JSON PascalCase wymagane przez aktualne modele Angular, niezależnie od globalnej polityki serializatora ASP.NET Core.
- Testuj każdą operację filtra, wartości `null`, typy wyliczeniowe, identyfikatory GUID, daty, liczby, kolekcje JSON, zagnieżdżone ścieżki, oba formaty sortowania, niepoprawne wejście, granice paginacji i rzeczywiste wykonanie przez dostawcę EF Core.
- Użyj skilla `develop-sebkuw-queryable-processor` i zweryfikuj `tests/sebkuw.QueryableProcessor.Tests`.
