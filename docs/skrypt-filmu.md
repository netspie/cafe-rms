# Skrypt lektorski do filmu — CafeRMS

Czytaj na głos, spokojnie i wyraźnie. Nawiasy [klik: ...] to podpowiedzi, kiedy przełączać widok — nie czytaj ich.

---

**[start — panel logowania web]**

Nazywam się [Imię Nazwisko]. Prezentuję projekt inżynierski CafeRMS — system zarządzania kawiarnią z obsługą zamówień online i organizacją wydarzeń. System składa się z panelu konfiguracyjnego dla pracowników, aplikacji mobilnej dla klientów oraz wspólnego API.

**[klik: zaloguj admin@shiba.pl]**

Zaczynam od panelu dla pracowników. Menu boczne dzieli się na katalog, konfigurację sprzedaży, operacje, raporty oraz ustawienia.

**[klik: Produkty → otwórz Matcha latte]**

Sercem systemu jest katalog produktów. Otwieram przykładowy produkt. Każdy produkt ma nazwę, opis i zdjęcie, przypisaną stawkę podatku VAT, a także alergeny i tagi. Ceny ustalane są w ujęciu brutto, osobno dla każdej grupy cenowej — dzięki temu ten sam produkt może mieć inną cenę w cenniku standardowym i promocyjnym. Do produktu przypisuję również grupy modyfikatorów, na przykład rozmiar czy rodzaj mleka, które klient będzie wybierał przy zamówieniu.

**[klik: edycja / upload zdjęcia]**

Zdjęcie produktu wgrywam bezpośrednio w panelu, a ceny ustawiam w każdej grupie cenowej.

**[klik: przeklikaj Tagi, Alergeny, Grupy modyfikatorów, Modyfikatory, Listy produktów]**

W katalogu definiuję też tagi produktów, słownik alergenów, grupy modyfikatorów i pojedyncze modyfikatory. Z produktów tworzę listy menu.

**[klik: Stawki VAT, Grupy cenowe, Kanały sprzedaży]**

W konfiguracji sprzedaży ustalam stawki podatku, grupy cenowe oraz kanały sprzedaży wraz z przypisanymi im grupami cenowymi.

**[klik: Kody promocyjne → otwórz jeden]**

Definiuję również kody promocyjne — z rabatem procentowym, okresem ważności i limitem użyć.

**[klik: Firma i lokal → przełącz domyślne menu]**

W ustawieniach lokalu mogę w każdej chwili przełączyć aktywne menu między przygotowanymi listami produktów.

---

**[klik: aplikacja mobilna — ekran logowania]**

Przechodzę do aplikacji mobilnej dla klienta. Klient rejestruje się i loguje.

**[klik: menu]**

Po zalogowaniu widzi menu ze zdjęciami i cenami brutto.

**[klik: otwórz produkt kawowy]**

Wchodzi w szczegóły produktu, gdzie widzi opis i alergeny oraz wybiera modyfikatory — po jednym z każdej grupy — i ilość.

**[klik: Dodaj do zamówienia → koszyk → checkout]**

Dodaje produkt do koszyka i przechodzi do podsumowania. Wybiera kanał sprzedaży lub stolik.

**[klik: wpisz kod promocyjny, użyj punktów]**

Przy zamówieniu może wykorzystać kod promocyjny oraz punkty lojalnościowe, które pokrywają maksymalnie połowę wartości zamówienia. Podsumowanie prezentowane jest w cenach brutto.

**[klik: złóż zamówienie]**

Składam zamówienie.

**[klik: historia zamówień, lojalność, profil]**

Klient ma wgląd w historię swoich zamówień, saldo punktów lojalnościowych oraz profil.

---

**[klik: wróć do panelu web — pojawia się powiadomienie]**

Wracam do panelu. Zamówienie złożone przed chwilą w aplikacji pojawia się natychmiast, z powiadomieniem dźwiękowym.

**[klik: Zamówienia → otwórz zamówienie]**

Otwieram to zamówienie. Widzę jego pozycje wraz z wybranymi modyfikatorami, przypisany stolik oraz sumę. Personel może zamówienie obsłużyć i zamknąć.

---

**[klik: Wydarzenia → otwórz dzisiejsze wydarzenie]**

Osobnym procesem jest organizacja wydarzeń. Wydarzenie ma swoje dni, status oraz odrębny cennik i listę produktów obowiązujące w dniu wydarzenia.

**[klik: Pobierz potwierdzenie]**

Dla wydarzenia generuję dokument potwierdzający.

**[klik: sekcja szablonu na dole strony Wydarzeń]**

Potwierdzenie powstaje na podstawie edytowalnego szablonu Word, który mogę w systemie podmienić.

**[klik: mobile → Wydarzenia, potem menu z ceną wydarzenia]**

W aplikacji klient widzi wydarzenia, a produkty nimi objęte mają obniżoną cenę wydarzenia.

---

**[klik: Raporty → Sprzedaż, wg produktów, Frekwencja]**

System generuje raporty biznesowe — sprzedaży, sprzedaży według produktów oraz frekwencji na wydarzeniach.

**[klik: eksport PDF / Excel]**

Każdy raport można wyeksportować do PDF oraz do Excela.

---

**[klik: Użytkownicy, Role]**

Na koniec pokazuję kontrolę dostępu. Uprawnienia definiuję w rolach.

**[klik: wyloguj → zaloguj barista@shiba.pl]**

Loguję się jako pracownik z ograniczoną rolą. Menu boczne jest teraz krótsze — zawiera tylko dozwolone obszary.

**[klik: wpisz w pasku adresu /admin/tax-rates]**

Przy próbie ręcznego wejścia na niedozwolony widok następuje przekierowanie. Dostęp jest egzekwowany nie tylko w interfejsie, ale też po stronie API.

---

**[koniec]**

Backend zbudowano na platformie .NET i bazie PostgreSQL, panel konfiguracyjny w Next.js, a aplikację mobilną w React Native. Dziękuję za uwagę.
