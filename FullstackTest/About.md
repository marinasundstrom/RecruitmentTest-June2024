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

Jag valde först att hämta ut väderdatan sekventiellt i en foreach loop där jag väntade på varje anrop. Sedan gick jag över till att göra på separata trådar som väntas av ``Task.WhenAll``. Detta för att korta ner tiden för requestet iom alla anrop.

Jag använde också ``IAsyncEnumerable`` men jag byggde bort det för att det inte skulle funka med endpoints som returnerar olika statuskoder. Kändes fel att ha det i denna lösning iom att jag inte behöver strömma data.

Detta kan ses i historiken i Git.

## Intressanta saker om lösningen

* Använder API Endpoints (istället för API Controllers)
* Cachear response
* Temperatursidan i UI är en interaktiv Blazor WebAssembly komponent som körs lokalt i webbläsaren.
* Följer standarden med ``ProblemDetails``.

## Möjliga förbättringar

* API:et skulle kunna ta parametrar som väljer ut temperatur för ett visst kontor och tid. Men det krävs omskrivning och vidare optimering.
* Man skulle kunna cachea resultatet på servern också.

## Tilltänkta användningsfall för framtiden

Man vill kunna välja en viss tid och se vad temperaturen är för kontoren just då.

Man vill gå in och se prognosen för ett visst kontor. Kunna se en hel tidsserie.

## Möjliga sätt att testa

Det finns tester som testar endpoints. För fallen där tjänsterna fallerar, för att inte kan ladda kontoren, eller hämta data från SMHI.

Jag försöker att inte skriva tester för testernas skull. De ska vara meningsfulla.

Integrationstest mot SMHI för att se till att API-kontraktet gäller.