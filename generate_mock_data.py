#!/usr/bin/env python3


import json
import random
import os
from datetime import date, datetime, timedelta, time
from pathlib import Path

random.seed(42)

OUTPUT_DIR = Path(__file__).parent / "mocks"
OUTPUT_DIR.mkdir(exist_ok=True)





FIRST_NAMES_MALE = [
    "Jan", "Adam", "Piotr", "Michał", "Tomasz", "Krzysztof", "Marcin", "Łukasz",
    "Dawid", "Bartosz", "Rafał", "Jakub", "Szymon", "Mateusz", "Wojciech",
    "Grzegorz", "Patryk", "Damian", "Karol", "Sebastian", "Maciej", "Paweł",
    "Przemysław", "Artur", "Mariusz", "Adrian", "Konrad", "Filip", "Kamil", "Daniel",
]

FIRST_NAMES_FEMALE = [
    "Anna", "Maria", "Katarzyna", "Magdalena", "Agnieszka", "Małgorzata", "Ewa",
    "Aleksandra", "Joanna", "Monika", "Natalia", "Paulina", "Karolina", "Marta",
    "Dorota", "Barbara", "Justyna", "Dominika", "Weronika", "Izabela", "Elżbieta",
    "Agata", "Kinga", "Julia", "Zuzanna", "Patrycja", "Sylwia", "Wiktoria",
    "Olga", "Martyna",
]

LAST_NAMES = [
    "Kowalski", "Nowak", "Wiśniewski", "Wójcik", "Kowalczyk", "Kamiński",
    "Lewandowski", "Zieliński", "Szymański", "Woźniak", "Dąbrowski", "Kozłowski",
    "Jankowski", "Mazur", "Kwiatkowski", "Krawczyk", "Piotrowski", "Grabowski",
    "Nowakowski", "Pawłowski", "Michalski", "Adamczyk", "Zając", "Wieczorek",
    "Jabłoński", "Król", "Majewski", "Olszewski", "Jaworski", "Wróbel",
    "Malinowski", "Dudek", "Witkowski", "Walczak", "Stępień", "Górski",
    "Rutkowski", "Michalak", "Sikora", "Ostrowski", "Baran", "Duda",
    "Marciniak", "Borkowski", "Czarnecki", "Sawicki", "Sokołowski", "Urbański",
    "Tomaszewski", "Zawadzki",
]

CITIES = [
    "Warszawa", "Kraków", "Wrocław", "Poznań", "Gdańsk", "Szczecin",
    "Łódź", "Lublin", "Bydgoszcz", "Katowice", "Rzeszów", "Białystok",
    "Gdynia", "Częstochowa", "Radom", "Toruń",
]

STREETS = [
    "ul. Marszałkowska", "ul. Akademicka", "Aleje Jerozolimskie",
    "ul. Nowy Świat", "ul. Puławska", "ul. Mokotowska", "ul. Legnicka",
    "ul. Grunwaldzka", "ul. Kościuszki", "ul. Piłsudskiego",
    "ul. Długa", "ul. Krótka", "ul. Szeroka", "ul. Polna",
    "ul. Słoneczna", "ul. Kwiatowa", "ul. Leśna", "ul. Ogrodowa",
]

CLINIC_NAMES_PREFIX = [
    "Centrum Medyczne", "Klinika Zdrowie", "Przychodnia Specjalistyczna",
    "Centrum Zdrowia", "Poradnia", "Instytut Medyczny", "Klinika",
    "NZOZ", "Przychodnia Lekarska", "Centrum Diagnostyczne",
    "Ośrodek Zdrowia", "Przychodnia Rodzinna", "Specjalistyczna Przychodnia",
    "Centrum Rehabilitacji", "Przychodnia Miejska",
]

SPECS = [
    {"id": 1, "name": "Alergolog", "desc": "Diagnostyka i leczenie alergii"},
    {"id": 2, "name": "Anestezjolog", "desc": "Znieczulenia i intensywna terapia"},
    {"id": 3, "name": "Chirurg ogólny", "desc": "Zabiegi operacyjne"},
    {"id": 4, "name": "Internista", "desc": "Choroby wewnętrzne, diagnostyka ogólna"},
    {"id": 5, "name": "Dermatolog", "desc": "Choroby skóry, włosów i paznokci"},
    {"id": 6, "name": "Diabetolog", "desc": "Leczenie cukrzycy i powikłań"},
    {"id": 7, "name": "Endokrynolog", "desc": "Zaburzenia hormonalne i tarczyca"},
    {"id": 8, "name": "Gastroenterolog", "desc": "Choroby układu pokarmowego"},
    {"id": 9, "name": "Ginekolog", "desc": "Zdrowie kobiet, ciąża"},
    {"id": 10, "name": "Kardiolog", "desc": "Diagnostyka i leczenie chorób serca"},
    {"id": 11, "name": "Lekarz medycyny pracy", "desc": "Badania profilaktyczne pracowników"},
    {"id": 12, "name": "Lekarz medycyny rodzinnej", "desc": "Podstawowa opieka zdrowotna"},
    {"id": 13, "name": "Neurolog", "desc": "Choroby układu nerwowego"},
    {"id": 14, "name": "Okulista", "desc": "Choroby oczu i wady wzroku"},
    {"id": 15, "name": "Onkolog", "desc": "Diagnostyka i leczenie nowotworów"},
    {"id": 16, "name": "Ortopeda", "desc": "Choroby kości, stawów i kręgosłupa"},
    {"id": 17, "name": "Pediatra", "desc": "Leczenie dzieci i młodzieży"},
    {"id": 18, "name": "Psychiatra", "desc": "Zdrowie psychiczne i zaburzenia"},
    {"id": 19, "name": "Pulmonolog", "desc": "Choroby płuc i układu oddechowego"},
    {"id": 20, "name": "Urolog", "desc": "Choroby układu moczowego"},
]

APPOINTMENT_TYPES = [
    {"id": 1, "name": "Konsultacja pierwszorazowa", "desc": "Pierwsza wizyta u specjalisty", "price": 220.0, "duration": 30},
    {"id": 2, "name": "Wizyta kontrolna", "desc": "Wizyta kontrolna po leczeniu", "price": 170.0, "duration": 20},
    {"id": 3, "name": "Teleporada", "desc": "Konsultacja online", "price": 140.0, "duration": 20},
    {"id": 4, "name": "Badanie USG", "desc": "Badanie ultrasonograficzne", "price": 300.0, "duration": 40},
    {"id": 5, "name": "Zabieg diagnostyczny", "desc": "Drobne zabiegi diagnostyczne", "price": 400.0, "duration": 60},
    {"id": 6, "name": "Wizyta krótka", "desc": "Ekspresowa konsultacja", "price": 100.0, "duration": 15},
    {"id": 7, "name": "Badania laboratoryjne", "desc": "Pobranie materiału do badań", "price": 150.0, "duration": 10},
    {"id": 8, "name": "Konsultacja rozszerzona", "desc": "Szczegółowa diagnostyka", "price": 350.0, "duration": 45},
]

APPT_STATUSES = [
    "PendingConfirmation", "AwaitingPayment", "Confirmed", "Completed",
    "Cancelled", "Unpaid", "AwaitingOnSitePayment",
]

PAYMENT_METHODS = ["PayU", "Offline", "Blik", "Karta"]
PAYMENT_STATUSES = ["Pending", "Paid", "Failed", "Refunded"]

NOTIFICATION_TYPES = ["AppointmentReminder", "PaymentConfirmation", "AppointmentCancelled",
                       "NewAppointment", "DoctorMessage", "SystemNotification"]
NOTIFICATION_STATUSES = ["Sent", "Pending", "Failed"]





def random_date(start: date, end: date) -> date:
    delta = (end - start).days
    return start + timedelta(days=random.randint(0, delta))

def random_datetime(start: datetime, end: datetime) -> datetime:
    delta = (end - start).total_seconds()
    return start + timedelta(seconds=random.randint(0, int(delta)))

def gender_from_name(name: str) -> str:
    return "Female" if name in FIRST_NAMES_FEMALE else "Male"

def generate_phone() -> str:
    return f"+48{random.randint(100000000, 999999999)}"

def generate_email(first: str, last: str, domain: str = "example.com") -> str:
    return f"{first.lower()}.{last.lower()}@{domain}"





def generate_users(num_doctors: int = 15, num_patients: int = 35, num_admins: int = 2):
    users = []
    used = set()

    def make_user(uid, first, last, role, is_active=True):
        email = generate_email(first, last, "medreserve.pl" if role != "Patient" else "example.com")
        birth = random_date(date(1960, 1, 1), date(2000, 12, 31))
        gender = gender_from_name(first)
        return {
            "id": uid,
            "email": email,
            "firstName": first,
            "lastName": last,
            "phoneNumber": generate_phone(),
            "birthDate": birth.isoformat(),
            "gender": gender,
            "isActive": is_active,
            "role": role,
        }


    for i in range(num_doctors):
        uid = f"doc-{i+1}"
        first = random.choice(FIRST_NAMES_MALE if random.random() > 0.4 else FIRST_NAMES_FEMALE)
        last = random.choice(LAST_NAMES)
        key = f"{first}-{last}"
        while key in used:
            first = random.choice(FIRST_NAMES_MALE if random.random() > 0.4 else FIRST_NAMES_FEMALE)
            last = random.choice(LAST_NAMES)
            key = f"{first}-{last}"
        used.add(key)
        users.append(make_user(uid, first, last, "Doctor"))


    for i in range(num_patients):
        uid = f"pat-{i+1}"
        first = random.choice(FIRST_NAMES_MALE if random.random() > 0.5 else FIRST_NAMES_FEMALE)
        last = random.choice(LAST_NAMES)
        key = f"{first}-{last}"
        while key in used:
            first = random.choice(FIRST_NAMES_MALE if random.random() > 0.5 else FIRST_NAMES_FEMALE)
            last = random.choice(LAST_NAMES)
            key = f"{first}-{last}"
        used.add(key)
        users.append(make_user(uid, first, last, "Patient"))


    for i in range(num_admins):
        uid = f"adm-{i+1}"
        first = random.choice(FIRST_NAMES_MALE)
        last = random.choice(LAST_NAMES)
        users.append(make_user(uid, first, last, "Admin"))

    return users






def generate_clinics(num: int = 15):
    clinics = []
    for i in range(num):
        city = random.choice(CITIES)
        prefix = random.choice(CLINIC_NAMES_PREFIX)
        street = random.choice(STREETS)
        nr = random.randint(1, 200)
        lat = round(50.0 + random.random() * 4.5, 6)
        lng = round(14.0 + random.random() * 10.0, 6)
        clinics.append({
            "clinicId": i + 1,
            "name": f"{prefix} {city}",
            "streetAddress": f"{street} {nr}",
            "city": city,
            "phoneNumber": generate_phone(),
            "email": generate_email("kontakt", f"klinika{i+1}", "med.pl"),
            "isActive": True,
            "description": f"Nowoczesna placówka medyczna oferująca szeroki zakres usług w {city.lower()}.",
            "openingHours": "Pon-Pt 07:00-20:00, Sob 08:00-14:00",
            "latitude": lat,
            "longitude": lng,
        })
    return clinics






def generate_specializations():
    return [
        {"specializationId": s["id"], "name": s["name"], "description": s["desc"]}
        for s in SPECS
    ]






def generate_appointment_types():
    return [
        {
            "appointmentTypeId": t["id"],
            "name": t["name"],
            "description": t["desc"],
            "basePrice": t["price"],
            "durationMinutes": t["duration"],
        }
        for t in APPOINTMENT_TYPES
    ]






def generate_doctors(num: int = 15):
    doctors = []
    bio_samples = [
        "Specjalista z wieloletnim doświadczeniem w diagnostyce i leczeniu.",
        "Absolwent Warszawskiego Uniwersytetu Medycznego. Staż kliniczny w Berlinie.",
        "Członek Polskiego Towarzystwa Lekarskiego. Regularnie uczestniczy w konferencjach międzynarodowych.",
        "Laureat nagrody za wybitne osiągnięcia w dziedzinie medycyny.",
        "Specjalizuje się w nowoczesnych metodach leczenia. Prowadzi badania naukowe.",
        "Doświadczony klinicysta z 15-letnią praktyką w szpitalu klinicznym.",
        "Autor wielu publikacji naukowych w czasopismach medycznych.",
        "Ukończył studia z wyróżnieniem. Stale podnosi kwalifikacje na kursach.",
        "Specjalista z certyfikatem europejskim. Przyjmuje pacjentów w języku angielskim.",
        "Pasjonat nowoczesnych technologii medycznych i telemedycyny.",
    ]
    for i in range(num):
        profile_img = f"/api/images/profiles/doc-{i+1}.jpg" if random.random() > 0.5 else None
        doctors.append({
            "doctorId": i + 1,
            "userId": f"doc-{i+1}",
            "licenseNumber": f"{random.randint(1000000, 9999999)}",
            "bio": random.choice(bio_samples),
            "profileImageUrl": profile_img,
        })
    return doctors






def generate_clinic_doctors(num_clinics: int, num_doctors: int):
    items = []
    seen = set()

    for doc_id in range(1, num_doctors + 1):
        n = random.randint(1, 3)
        clinic_ids = random.sample(range(1, num_clinics + 1), min(n, num_clinics))
        for cid in clinic_ids:
            key = (cid, doc_id)
            if key in seen:
                continue
            seen.add(key)
            items.append({
                "clinicId": cid,
                "doctorId": doc_id,
                "isOwner": len([x for x in items if x["doctorId"] == doc_id]) == 0 and random.random() < 0.2,
            })
    return items






def generate_doctor_specializations(num_doctors: int):
    items = []
    seen = set()
    for doc_id in range(1, num_doctors + 1):
        n = random.randint(1, 3)
        spec_ids = random.sample(range(1, len(SPECS) + 1), n)
        for sid in spec_ids:
            key = (doc_id, sid)
            if key in seen:
                continue
            seen.add(key)
            items.append({"doctorId": doc_id, "specializationId": sid})
    return items






def generate_doctor_appointment_types(num_doctors: int):
    items = []
    seen = set()
    for doc_id in range(1, num_doctors + 1):
        n = random.randint(3, len(APPOINTMENT_TYPES))
        type_ids = random.sample(range(1, len(APPOINTMENT_TYPES) + 1), n)
        for tid in type_ids:
            key = (doc_id, tid)
            if key in seen:
                continue
            seen.add(key)
            items.append({"doctorId": doc_id, "appointmentTypeId": tid})
    return items






def generate_doctor_schedules(num_doctors: int, clinic_doctors: list):
    items = []
    schedule_id = 1
    start_date = datetime(2026, 1, 1, tzinfo=None)


    doc_clinics = {}
    for cd in clinic_doctors:
        doc_clinics.setdefault(cd["doctorId"], []).append(cd["clinicId"])

    time_options = [
        ("07:00", "15:00"),
        ("08:00", "16:00"),
        ("09:00", "17:00"),
        ("10:00", "18:00"),
        ("08:00", "14:00"),
        ("12:00", "20:00"),
        ("07:30", "13:30"),
        ("07:00", "13:00"),
    ]

    for doc_id in range(1, num_doctors + 1):
        for clinic_id in doc_clinics.get(doc_id, []):
            workdays = random.sample(range(1, 6), random.randint(3, 5))
            for day in sorted(workdays):
                start_t, end_t = random.choice(time_options)
                items.append({
                    "scheduleId": schedule_id,
                    "doctorId": doc_id,
                    "clinicId": clinic_id,
                    "dayOfWeek": day,
                    "startTime": start_t,
                    "endTime": end_t,
                    "validFrom": start_date.isoformat() + "Z",
                    "validTo": None,
                    "isActive": True,
                })
                schedule_id += 1
    return items






def generate_appointments(num: int, num_patients: int, num_doctors: int,
                          doctor_appt_types: list, doctor_schedules: list):
    items = []
    now = datetime(2026, 6, 18)
    base = datetime(2026, 5, 1, tzinfo=None)
    end = datetime(2026, 8, 31, tzinfo=None)


    doc_types = {}
    for dt in doctor_appt_types:
        doc_types.setdefault(dt["doctorId"], []).append(dt["appointmentTypeId"])


    type_duration = {t["id"]: t["duration"] for t in APPOINTMENT_TYPES}

    schedule_lookup = {}
    for s in doctor_schedules:
        key = (s["doctorId"], s["dayOfWeek"])
        if key not in schedule_lookup:
            schedule_lookup[key] = []
        schedule_lookup[key].append(s)

    for appt_id in range(1, num + 1):
        doctor_id = random.randint(1, num_doctors)
        patient_id = random.randint(1, num_patients)


        available_types = doc_types.get(doctor_id, [1])
        type_id = random.choice(available_types)


        duration = type_duration.get(type_id, 30)


        created = random_datetime(base, min(now, end))
        appt_date = created.date() + timedelta(days=random.randint(0, 14))


        day_of_week = appt_date.isoweekday()
        schedules_for_day = schedule_lookup.get((doctor_id, day_of_week), [])

        if not schedules_for_day:
            continue

        schedule = random.choice(schedules_for_day)
        start_h, start_m = map(int, schedule["startTime"].split(":"))
        end_h, end_m = map(int, schedule["endTime"].split(":"))


        max_start_min = (end_h * 60 + end_m) - duration
        min_start_min = start_h * 60 + start_m
        if max_start_min <= min_start_min:
            continue

        slot_min = random.randrange(min_start_min, max_start_min, 15)
        start_time = time(slot_min // 60, slot_min % 60)


        status = random.choice(APPT_STATUSES)


        confirmed_at = None
        completed_at = None
        cancelled_at = None
        if status == "Confirmed":
            confirmed_at = (created + timedelta(hours=random.randint(1, 24))).isoformat() + "Z"
        elif status == "Completed":
            confirmed_at = (created + timedelta(hours=random.randint(1, 24))).isoformat() + "Z"
            completed_at = (datetime.combine(appt_date, start_time) + timedelta(minutes=duration)).isoformat() + "Z"
        elif status == "Cancelled":
            cancelled_at = (created + timedelta(hours=random.randint(1, 48))).isoformat() + "Z"

        items.append({
            "appointmentId": appt_id,
            "userId": f"pat-{patient_id}",
            "doctorId": doctor_id,
            "appointmentDate": appt_date.isoformat(),
            "startTime": start_time.isoformat(),
            "appointmentTypeId": type_id,
            "appointmentTypeDurationMinutes": duration,
            "status": status,
            "doctorNotes": "Proszę zabrać ze sobą wyniki poprzednich badań." if random.random() < 0.3 else None,
            "cancellationReason": "Pacjent zrezygnował z wizyty." if status == "Cancelled" else None,
            "createdAt": created.isoformat() + "Z",
            "updatedAt": created.isoformat() + "Z",
            "confirmedAt": confirmed_at,
            "completedAt": completed_at,
            "cancelledAt": cancelled_at,
        })

    return items






def generate_payments(appointments: list):
    items = []
    p_id = 1
    for appt in appointments:
        method = random.choice(PAYMENT_METHODS)
        if method == "PayU":
            method = "PayU"
        elif method == "Karta":
            method = "Offline"

        created = datetime.fromisoformat(appt["createdAt"].replace("Z", ""))
        status = "Paid" if appt["status"] in ("Confirmed", "Completed") else \
                 "Cancelled" if appt["status"] == "Cancelled" else \
                 random.choice(["Pending", "Failed"])

        paid_at = None
        if status == "Paid":
            paid_at = (created + timedelta(minutes=random.randint(5, 60))).isoformat() + "Z"


        apt_price = next((t["price"] for t in APPOINTMENT_TYPES if t["id"] == appt["appointmentTypeId"]), 220.0)

        items.append({
            "paymentId": p_id,
            "appointmentId": appt["appointmentId"],
            "amount": apt_price,
            "currency": "PLN",
            "method": method,
            "status": status,
            "createdAt": created.isoformat() + "Z",
            "updatedAt": (created + timedelta(minutes=random.randint(5, 30))).isoformat() + "Z",
            "paidAt": paid_at,
        })
        p_id += 1
    return items






NOTIFICATION_TEMPLATES = [
    ("AppointmentReminder", "Przypomnienie o wizycie", "Przypominamy o jutrzejszej wizycie. Prosimy o punktualne przybycie."),
    ("PaymentConfirmation", "Potwierdzenie płatności", "Twoja płatność za wizytę została przyjęta."),
    ("AppointmentCancelled", "Wizyta odwołana", "Twoja wizyta została odwołana. Skontaktuj się z przychodnią."),
    ("NewAppointment", "Nowa wizyta", "Twoja wizyta została pomyślnie zarejestrowana."),
    ("DoctorMessage", "Wiadomość od lekarza", "Lekarz przesyła zalecenia po wizycie."),
    ("SystemNotification", "Informacja systemowa", "Twoje konto zostało zweryfikowane."),
]

def generate_notifications(appointments: list, users: list, num_extra: int = 50):
    items = []
    n_id = 1
    valid_appt_ids = [a["appointmentId"] for a in appointments]


    for appt in appointments:
        tpl = random.choice(NOTIFICATION_TEMPLATES)
        created = datetime.fromisoformat(appt["createdAt"].replace("Z", ""))
        status = random.choice(NOTIFICATION_STATUSES)
        items.append({
            "notificationId": n_id,
            "userId": appt["userId"],
            "appointmentId": appt["appointmentId"],
            "type": tpl[0],
            "subject": tpl[1],
            "content": tpl[2],
            "status": status,
            "createdAt": created.isoformat() + "Z",
            "sentAt": (created + timedelta(seconds=random.randint(1, 60))).isoformat() + "Z" if status == "Sent" else None,
            "failureReason": "Serwer poczty niedostępny." if status == "Failed" else None,
        })
        n_id += 1


    for _ in range(num_extra):
        tpl = random.choice(NOTIFICATION_TEMPLATES)
        patient_id = random.randint(1, 35)
        appt_id = random.choice(valid_appt_ids) if random.random() < 0.5 else None
        created = random_datetime(datetime(2026, 5, 1), datetime(2026, 6, 18))
        status = random.choice(NOTIFICATION_STATUSES)
        items.append({
            "notificationId": n_id,
            "userId": f"pat-{patient_id}",
            "appointmentId": appt_id,
            "type": tpl[0],
            "subject": tpl[1],
            "content": tpl[2],
            "status": status,
            "createdAt": created.isoformat() + "Z",
            "sentAt": (created + timedelta(seconds=1)).isoformat() + "Z" if status == "Sent" else None,
            "failureReason": "Limit wiadomości przekroczony." if status == "Failed" else None,
        })
        n_id += 1

    return items






def generate_offline_approvals(payments: list, num_admins: int = 2):
    items = []
    a_id = 1
    offline_payments = [p for p in payments if p["method"] == "Offline" and p["status"] in ("Paid", "Pending")]
    for p in offline_payments[:30]:
        updated = datetime.fromisoformat(p["updatedAt"].replace("Z", ""))
        decision = random.choice(["Approved", "Rejected", "AwaitingReview"])
        items.append({
            "approvalId": a_id,
            "paymentId": p["paymentId"],
            "approvedByUserId": f"adm-{random.randint(1, num_admins)}",
            "decision": decision,
            "decisionDate": (updated + timedelta(minutes=random.randint(5, 120))).isoformat() + "Z",
            "comment": "Opłacono w recepcji." if decision == "Approved" else None,
        })
        a_id += 1
    return items






def write_json(filename: str, data: list):
    path = OUTPUT_DIR / filename
    with open(path, "w", encoding="utf-8") as f:
        json.dump(data, f, indent=2, ensure_ascii=False)
    print(f"  ✅ {filename} ({len(data)} wpisów)")

def main():
    print("🔧 MedReserve Mock Data Generator")
    print(f"📁 Output: {OUTPUT_DIR}\n")

    NUM_DOCTORS = 15
    NUM_PATIENTS = 35
    NUM_ADMINS = 2
    NUM_CLINICS = 15
    NUM_APPOINTMENTS = 200


    users = generate_users(NUM_DOCTORS, NUM_PATIENTS, NUM_ADMINS)
    write_json("users.json", users)

    clinics = generate_clinics(NUM_CLINICS)
    write_json("clinics.json", clinics)

    specs = generate_specializations()
    write_json("specializations.json", specs)

    appt_types = generate_appointment_types()
    write_json("appointment_types.json", appt_types)

    doctors = generate_doctors(NUM_DOCTORS)
    write_json("doctors.json", doctors)

    clinic_doctors = generate_clinic_doctors(NUM_CLINICS, NUM_DOCTORS)
    write_json("clinic_doctors.json", clinic_doctors)

    doc_specs = generate_doctor_specializations(NUM_DOCTORS)
    write_json("doctor_specializations.json", doc_specs)

    doc_appt_types = generate_doctor_appointment_types(NUM_DOCTORS)
    write_json("doctor_appointment_types.json", doc_appt_types)

    schedules = generate_doctor_schedules(NUM_DOCTORS, clinic_doctors)
    write_json("doctor_schedules.json", schedules)

    appointments = generate_appointments(NUM_APPOINTMENTS, NUM_PATIENTS, NUM_DOCTORS,
                                          doc_appt_types, schedules)
    write_json("appointments.json", appointments)

    payments = generate_payments(appointments)
    write_json("payments.json", payments)

    notifications = generate_notifications(appointments, users, num_extra=50)
    write_json("notifications.json", notifications)

    approvals = generate_offline_approvals(payments, NUM_ADMINS)
    write_json("offline_payment_approvals.json", approvals)

    print(f"\n🎉 Wygenerowano łącznie {sum(len(x) for x in [users, clinics, specs, appt_types, doctors, clinic_doctors, doc_specs, doc_appt_types, schedules, appointments, payments, notifications, approvals])} rekordów.")

if __name__ == "__main__":
    main()
