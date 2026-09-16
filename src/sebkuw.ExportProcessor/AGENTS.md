# Instrukcje dla sebkuw.ExportProcessor

- Zachowuj eksport CSV strumieniowy i niezależny od domeny, endpointu oraz sposobu przechowywania pliku.
- Filtry i sortowanie stosuj przed projekcją, a paginację zgodnie z jawnym zakresem eksportu.
- Eksportuj wyłącznie kolumny zdefiniowane przez aplikację w `CsvExportMap<T>`; nazwy otrzymane od klienta traktuj jako wybór z tej listy.
- Publiczne operacje I/O muszą przyjmować `CancellationToken` i pozostawiać strumień wyjściowy otwarty.
- Domyślnie zabezpieczaj komórki przed interpretacją formuł arkusza kalkulacyjnego.
- Utrzymuj kod w katalogach `Configuration`, `Extensions`, `Mapping` i `Models`, zachowując publiczne typy w namespace `sebkuw.ExportProcessor`.
- Testuj filtrowanie, sortowanie, oba zakresy paginacji, wybór kolumn, cytowanie CSV, kulturę, limity, anulowanie i niepoprawne argumenty.
