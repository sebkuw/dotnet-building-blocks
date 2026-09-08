---
name: develop-sebkuw-exception-processor
description: Utrzymuj mapowanie wyjątków, oprogramowanie pośredniczące API, ustrukturyzowane błędy JSON, logowanie i identyfikatory korelacji w sebkuw.ExceptionProcessor. Używaj przy zmianach typów wyjątków, mapowania statusów, pól odpowiedzi błędu, kolejności lub zachowania komponentów pośredniczących, adapterów logowania, ustawień NLog, testów, dokumentacji albo informacji o wydaniu.
---

# Rozwijanie procesora wyjątków

## Przebieg pracy

1. Przeczytaj główny i lokalny `AGENTS.md`, dokumentację paczki, `docs/angular-shared-components-contract.md`, cały kod wyjątków i oprogramowania pośredniczącego oraz testy wyjątków.
2. Utrzymuj pola JSON `code`, `httpCode`, `mainText`, `description`, `timestamp` i `traceId` jako stabilne oraz bezpieczne do wyświetlenia w interfejsie użytkownika.
3. Zachowaj jeden identyfikator korelacji w nagłówku żądania, elementach kontekstu, nagłówku i treści odpowiedzi oraz logach.
4. Utrzymuj wymienialność mechanizmu logowania. Nie uzależniaj publicznego kontraktu wyjątków od typów właściwych NLog.
5. Nie ujawniaj klientom śladów stosu, sekretów, szczegółów infrastruktury ani niezweryfikowanych komunikatów wyjątków.
6. Testuj każde wbudowane mapowanie, wyjątki własne i nieznane, dokładne nazwy JSON, status i typ zawartości, istniejący, brakujący i pusty identyfikator korelacji oraz cały potok oprogramowania pośredniczącego.
7. Zaktualizuj README i sekcję `Unreleased` dziennika zmian, a następnie uruchom testy wyjątków, kompilację Release, pakowanie i kontrolę paczki.
