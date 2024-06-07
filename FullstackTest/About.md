# About

## Plan

Detta behöver jag:

* Geocoordinater för alla itm8s svenska kontor
* API:et för att hämta ut väderdata (temp, hög el låg) baserat på koordinaterna till kontoren

Detta ska byggas:

* Web API för att utföra logiken med att hämta data, och presentera i ett vänligt format (JSON)
* UI för att presentera datan

Teknologier som jag använder:

* ASP.NET Core
* Blazor

Kolla upp hur SMHIs API funkar.

Verkar vara enkelt:

``
https://opendata-download-metfcst.smhi.se/api/category/pmp3g/version/2/geotype/point/lon/16.158/lat/58.5812/data.json
``

Man anger koordinaterna i URL:en.

Sen bara hämta ut parametrarna från resultatet.

## Mål

Inte göra det för komplicerat.

## Indata: Kontoren

Jag har letat upp Itm8 kontoren i Sverige och i Danmark, och angivit dem (deras koordinater) i JSON-format i filen itm8-offices.json. Notera att det kanske inte är exakt alla.

## Design

ASP.NET Core Web API och en Blazor app. Hostade i samma projekt.

Logiken sker i en API endpoing som kallas ``GetForecast``. I denna metod läses kontoren in från JSON-fil, och parsas manuellt. Sedan itereras denna lista, och för varje kontor så görs ett anrop till SMHIs API med respektive koordinater. Efter det skapas ett svarsobjekt med namn, plats, och erhållen temperatur.

Projektet har en OpenAPI specifikation (i YAML) som beskriver endpointen, från vilken en klient genereras med hjälp av NSwag,

Swagger UI servas på ``/openapi``

Klientappen är byggd med Blazor och använder Fluent UI som komponentramverk. Temperatursidan är simpel - listar resultatet som den begärt från servern. Sidan körs lokalt i webbläsaren, och den anropar Web API genom det genererade klienterna.

Datan i tabellen kan sorteras på klientsidan.

Inläsning av kontor hanteras i  serviceklassen ``OfficeDataReader``.

Anrop till SMHI hanteras i serviceklassen ``SmhiForecastsClient``. 

## Intressanta saker om lösningen

* Använder API Endpoints (istället för API Controllers)
* Använder ``IAsyncEnumerable`` när kontoren processas. 
* Cachear response
* Temperatursidan i UI är en interaktiv Blazor WebAssembly komponent som körs lokalt i webbläsaren.

## Möjliga förbättringar

* API:et skulle kunna ta parametrar som väljer ut temperatur för ett visst kontor och tid. Men det krävs omskrivning och vidare optimering.
* Man skulle kunna cachea resultatet på servern också.
* Requesten mot SMHI skulle kunna göras parallelt, med en ``Task.WhenAll``.
* Man kan testa beteendet i endpointen med mockade tjänster.
* Att WebAPI:er använder resultat-typer.
* Se till att Web API validerar mer, samt följer standarder som ``ProblemDetails``.

## Tilltänkta användningsfall för framtiden

Man vill kunna välja en viss tid och se vad temperaturen är för kontoren just då.

Man vill gå in och se prognosen för ett visst kontor. Kunna se en hel tidsserie.

## Möjliga sätt att testa

Nu är detta ett hyfsat enkelt program. Men om det vore mer komplext, med mer affärslogik, då skulle jag vilja verifiera beteendet. Och skriva automatiserade tester. För att undvika situationer där buggar introduceras.

Man kan normalt testa endpointen, verifiera beteendet utifrån input och output. Samma för service-klasserna.

Jag försöker att inte skriva tester för testernas skull. De ska vara meningsfulla.

Även när en bugg upptäcks så ska där finnas ett test som verifierar fixen.

Integrationstest mot SMHI för att se till att API-kontraktet gäller.