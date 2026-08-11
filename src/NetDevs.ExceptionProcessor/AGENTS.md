# Instrukcje dla NetDevs.ExceptionProcessor

- Utrzymuj stabilny, bezpieczny kontrakt JSON błędu: `code`, `httpCode`, `mainText`, `description`, `timestamp`, `traceId`.
- Nie zwracaj klientowi śladu stosu, sekretów ani szczegółów infrastruktury. Opisy błędów nieoczekiwanych powinny być bezpieczne.
- Zachowuj ten sam `X-Correlation-ID` w żądaniu, `HttpContext.Items`, odpowiedzi, treści błędu i logach.
- Komponent identyfikatora korelacji rejestruj przed globalnym komponentem obsługi wyjątków.
- Nie uzależniaj podstawowego kontraktu wyjątków od NLog; adapter logowania ma pozostać wymienialny.
- Testuj wszystkie mapowania statusów, wyjątki własne i nieznane, dokładny JSON, cały potok, istniejący, brakujący i pusty identyfikator korelacji oraz błąd po rozpoczęciu odpowiedzi.
- Użyj skilla `develop-netdevs-exception-processor` i zweryfikuj `tests/NetDevs.ExceptionProcessor.Tests`.
