module SSV.Types
  type SSV_Woche = {
    Montag : float
    Dienstag : float
    Mittwoch : float
    Donnerstag : float
    Freitag : float
    Samstag : float
    Sonntag : float
  }
  let wochenstunden (woche:SSV_Woche):float =
    woche.Montag +
    woche.Dienstag +
    woche.Mittwoch +
    woche.Donnerstag +
    woche.Freitag +
    woche.Samstag +
    woche.Sonntag

  type SSV_Wochen = SSV_Woche list // nonempty

  // später vereinheitlichen mit AZM
  // später nochmal über ZRG nachdenken
  type  Zeitraumgruppe = {
     Id : string
     Bezeichnung : string
  }
  type Verwendung =
    | Inklusiv
    | Exklusiv

  type Zeitraumgruppenzuordnung =
    | Immer
    | Spezifiziert of Map<Zeitraumgruppe,Verwendung>

  type SSV_Element = {
    Wochen : SSV_Wochen
    Zeitraumgruppe : Zeitraumgruppenzuordnung
  }

  type SSV_Element_Zuordnung = {
    Element : SSV_Element
    Prioritaet : int
  }

  type SSV = SSV_Element_Zuordnung list //nonempty list

  // später P-Plus global inkl Zeitraum Art (wöchentlich, monatlich, jährlich)
  type Beschaeftigungsumfang = {
    Stundenzahl : float
    X_Tagewoche : float
  }


  type Wochentag =
    | Montag
    | Dienstag
    | Mittwoch
    | Donnerstag
    | Freitag
    | Samstag
    | Sonntag


  type Model = SSV

  type Msg =
    | Tag_Stundenwert_geaendert of Wochentag*string
    | Woche_hinzufuegen


  let SSV_default() = {
    Montag = 0.
    Dienstag = 0.
    Mittwoch = 0.
    Donnerstag = 0.
    Freitag = 0.
    Samstag = 0.
    Sonntag = 0.
  }