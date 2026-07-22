# Scenariusz filmu — CafeRMS

Film demonstracyjny funkcjonalności. Pokaż WSZYSTKIE widoki (web + mobile) i przejścia między nimi.
Dokładnie omawiasz tylko produkty — resztę pokazujesz i podpisujesz jednym zdaniem. Mów głośno i wyraźnie.

Oznaczenia: [POKAŻ] = co klikasz, [MÓW] = co mówisz (możesz parafrazować).

---

## Przygotowanie (przed nagraniem)

- Reset danych: `docker compose -f src/api/docker-compose.yml down -v` → `up --build -d`
- Web na `localhost:3000`, mobile na emulatorze (`EXPO_PUBLIC_API_BASE_URL=http://10.0.2.2:5179 npm run start`)
- Sprawdź w panelu jeden kod promocyjny (użyjesz go na checkoucie)
- Nagrywaj ekran + głos; najlepiej jedno przejście

---

## 1. Wstęp (~10 s)

[MÓW] „Nazywam się [Imię Nazwisko], prezentuję projekt inżynierski CafeRMS — system zarządzania kawiarnią z obsługą zamówień online i organizacją wydarzeń. Składa się z panelu konfiguracyjnego dla pracowników, aplikacji mobilnej dla klientów oraz wspólnego API."

---

## 2. Panel — produkty i menu (GŁÓWNA CZĘŚĆ, mów dokładnie, ~2–3 min)

[POKAŻ] Zaloguj `admin@shiba.pl` / `Demo1234`. Przejedź wzrokiem po menu bocznym.
[MÓW] „Panel dzieli się na katalog, konfigurację sprzedaży, operacje, raporty i ustawienia."

[POKAŻ] **Produkty** → otwórz np. **Matcha latte**. Pokaż: zdjęcie, ceny per grupa cenowa, VAT, alergeny, tagi, grupy modyfikatorów.
[MÓW] „Każdy produkt ma cenę brutto ustalaną osobno dla każdej grupy cenowej, przypisaną stawkę VAT, alergeny, tagi oraz modyfikatory — na przykład rozmiar czy rodzaj mleka. Zdjęcia dodaje się bezpośrednio w panelu."

[POKAŻ] Edytuj produkt — wgraj zdjęcie (pokaż upload). Dodaj/zmień cenę w grupie cenowej.
[MÓW] „Zdjęcie wgrywam z dysku, ceny ustawiam per grupa cenowa."

---

## 3. Panel — reszta katalogu (szybko, pokaż i podpisz, ~40 s)

Przeklikaj kolejno, jedno zdanie na każdy:
- [POKAŻ] **Tagi** — [MÓW] „Tagi produktów."
- [POKAŻ] **Alergeny** — [MÓW] „Słownik alergenów."
- [POKAŻ] **Grupy modyfikatorów** — [MÓW] „Grupy wyborów, np. rozmiar, mleko."
- [POKAŻ] **Modyfikatory** — [MÓW] „Pojedyncze opcje w grupach."
- [POKAŻ] **Listy produktów** — [MÓW] „Z produktów składam listy menu."

---

## 4. Panel — konfiguracja sprzedaży (szybko, ~1 min)

- [POKAŻ] **Stawki VAT** — [MÓW] „Stawki podatku."
- [POKAŻ] **Grupy cenowe** — [MÓW] „Np. standard i promocja."
- [POKAŻ] **Kanały sprzedaży** — [MÓW] „Kanały i przypisane im grupy cenowe."
- [POKAŻ] **Kody promocyjne** → otwórz jeden. [MÓW] „Kody z rabatem procentowym, okresem ważności i limitem użyć." (zapamiętaj kod)

---

## 5. Panel — ustawienia i menu (szybko, ~40 s)

- [POKAŻ] **Firma i lokal** → pokaż dane + **przełączenie domyślnego menu**. [MÓW] „Aktywne menu lokalu mogę przełączać między listami."
- [POKAŻ] **Stoliki** — [MÓW] „Stoliki lokalu."
- [POKAŻ] **Lojalność** — [MÓW] „Podgląd punktów lojalnościowych klientów."

---

## 6. Aplikacja mobilna — klient zamawia (~2–3 min)

[POKAŻ] Zaloguj `user1@shiba.pl` / `Demo1234`. Pokaż też ekran **Rejestracji**.
[POKAŻ] **Menu** — zdjęcia, ceny brutto, cena wydarzenia.
[MÓW] „Klient widzi menu z cenami brutto."

[POKAŻ] Otwórz produkt kawowy → **Szczegóły**: alergeny, wybór modyfikatorów (jeden na grupę), ilość, ulubione → Dodaj do zamówienia.
[MÓW] „W szczegółach produktu klient wybiera modyfikatory — po jednym z każdej grupy — i dodaje do koszyka."

[POKAŻ] **Ulubione** → **Koszyk** → **Checkout**: kanał/stolik, wpisz kod promocyjny, użyj punktów lojalnościowych → złóż zamówienie.
[MÓW] „Przy zamówieniu można wykorzystać kod promocyjny oraz punkty lojalnościowe, które pokrywają maksymalnie połowę wartości. Podsumowanie jest w cenach brutto."

[POKAŻ] **Zamówienia** (lista → szczegóły z modyfikatorami), **Lojalność** (saldo + historia), **Profil**.
[MÓW] „Klient ma historię zamówień, saldo punktów i profil."

---

## 7. Panel — obsługa zamówienia w czasie rzeczywistym (~1 min)

[POKAŻ] Wróć do panelu web — pojawi się **powiadomienie o nowym zamówieniu** (toast + dźwięk). Wejdź w **Zamówienia** → otwórz to zamówienie → pozycje, wybrane modyfikatory, stolik, suma. Zamknij zamówienie.
[MÓW] „Zamówienie z aplikacji pojawia się w panelu natychmiast, z powiadomieniem dźwiękowym. Personel widzi pozycje z modyfikatorami i obsługuje zamówienie."

---

## 8. Wydarzenia (~1–2 min)

[POKAŻ] **Wydarzenia** → otwórz dzisiejsze wydarzenie → lista produktów i cennik wydarzenia, dni, status opublikowane. Kliknij **Pobierz potwierdzenie** (.docx). Zjedź na dół strony Wydarzeń → sekcja szablonu (pobierz / podmień .docx).
[MÓW] „Wydarzenia mają własny cennik i listę produktów obowiązujące w dniu wydarzenia. Potwierdzenie generuje się z edytowalnego szablonu Word, który można w systemie podmienić."

[POKAŻ] Mobile → **Wydarzenia** (lista → szczegóły). Wróć do menu → produkt wydarzenia ma przekreśloną cenę zwykłą.
[MÓW] „W aplikacji klient widzi wydarzenia, a produkty nimi objęte mają cenę wydarzenia."

---

## 9. Raporty (~1 min)

[POKAŻ] **Raporty** → **Sprzedaż** (wykres) → **Sprzedaż wg produktów** → **Frekwencja na wydarzeniach**. Przy jednym kliknij eksport **PDF** i **Excel**.
[MÓW] „System generuje raporty sprzedaży, sprzedaży według produktów oraz frekwencji na wydarzeniach, z eksportem do PDF i Excela."

---

## 10. Role i uprawnienia (~45 s)

[POKAŻ] **Użytkownicy** (lista) → **Role** (otwórz rolę: checkboxy uprawnień). Potem **Wyloguj** → zaloguj `barista@shiba.pl` / `Demo1234`. Pokaż, że menu boczne jest **krótsze**. W pasku adresu wpisz `localhost:3000/admin/tax-rates` → następuje przekierowanie.
[MÓW] „Uprawnienia definiuję w rolach. Zakres panelu zależy od roli — barista widzi mniej. Menu ukrywa niedostępne obszary, a dostęp jest dodatkowo egzekwowany po stronie API."

---

## 11. Zakończenie (~10 s)

[MÓW] „Backend zbudowano na .NET i PostgreSQL, panel na Next.js, a aplikację mobilną w React Native. Dziękuję."

---

## Checklista widoków (odhacz podczas nagrania)

WEB: Produkty · Tagi · Alergeny · Grupy modyfikatorów · Modyfikatory · Listy produktów · Stawki VAT · Grupy cenowe · Kanały sprzedaży · Kody promocyjne · Zamówienia · Wydarzenia · Stoliki · Lojalność · Raporty (Sprzedaż / wg produktów / Frekwencja) · Użytkownicy · Role · Firma i lokal · Moje konto · Logowanie

MOBILE: Logowanie · Rejestracja · Menu · Szczegóły produktu · Ulubione · Koszyk · Checkout · Zamówienia · Szczegóły zamówienia · Wydarzenia · Szczegóły wydarzenia · Lojalność · Profil
