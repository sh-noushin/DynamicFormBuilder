// Locale dictionary for public-form UI strings. Kept in a plain map (rather
// than @angular/localize / i18n-tools) because we only need three locales
// and no compile-time build variants - the picked language is per form, not
// per app build.
//
// Adding a locale: extend `PublicLocale`, add an entry to `PUBLIC_STRINGS`,
// and add the display label to `LOCALE_OPTIONS`. Missing keys fall back to
// English so a partial translation never blanks out UI.

export type PublicLocale = 'en' | 'es' | 'fr';

export type PublicStringKey =
  | 'loading'
  | 'unavailable'
  | 'thanksTitle'
  | 'thanksBody'
  | 'passwordGateSubtitle'
  | 'passwordLabel'
  | 'passwordChecking'
  | 'passwordUnlock'
  | 'passwordIncorrect'
  | 'passwordRequired'
  | 'passwordChanged'
  | 'closedTitle'
  | 'closedGeneric'
  | 'previous'
  | 'next'
  | 'submit'
  | 'submitting'
  | 'saveForLater'
  | 'savingDraft'
  | 'draftSavedTitle'
  | 'draftSavedBody'
  | 'copyLink'
  | 'done'
  | 'draftNote'
  | 'yourDetails'
  | 'yourName'
  | 'yourEmail'
  | 'fixHighlightedContinue'
  | 'fixHighlightedSubmit'
  | 'noLongerAcceptingResponses'
  | 'submitGenericError'
  | 'submittingTooQuickly'
  | 'duplicateEmail'
  | 'draftSaveError'
  | 'linkNotValid';

const en: Record<PublicStringKey, string> = {
  loading: 'Loading form…',
  unavailable: 'Form unavailable',
  thanksTitle: 'Thanks for your response!',
  thanksBody: 'Your submission has been recorded.',
  passwordGateSubtitle: 'This form is password protected. Enter the password to continue.',
  passwordLabel: 'Password',
  passwordChecking: 'Checking…',
  passwordUnlock: 'Unlock',
  passwordIncorrect: 'Incorrect password. Please try again.',
  passwordRequired: 'Please enter the password.',
  passwordChanged: 'The form password has changed. Please reload and enter it again.',
  closedTitle: 'This form is no longer accepting responses.',
  closedGeneric: 'This form is no longer accepting responses.',
  previous: 'Previous',
  next: 'Next',
  submit: 'Submit',
  submitting: 'Submitting…',
  saveForLater: 'Save & continue later',
  savingDraft: 'Saving…',
  draftSavedTitle: 'Your progress is saved',
  draftSavedBody: 'Return using this link to pick up where you left off:',
  copyLink: 'Copy link',
  done: 'Done',
  draftNote: 'The link is valid for 30 days. Keep it safe — anyone with the link can view or edit your response.',
  yourDetails: 'Your details (optional)',
  yourName: 'Your name',
  yourEmail: 'Your email',
  fixHighlightedContinue: 'Please fix the highlighted fields to continue.',
  fixHighlightedSubmit: 'Please fix the highlighted fields and submit again.',
  noLongerAcceptingResponses: 'This form is no longer accepting responses.',
  submitGenericError: 'Something went wrong sending your response.',
  submittingTooQuickly: 'You are submitting too quickly. Please wait a moment and try again.',
  duplicateEmail: 'A response has already been submitted from this email address.',
  draftSaveError: 'Could not save your draft. Please try again.',
  linkNotValid: 'This link is not valid.',
};

const es: Record<PublicStringKey, string> = {
  loading: 'Cargando formulario…',
  unavailable: 'Formulario no disponible',
  thanksTitle: '¡Gracias por tu respuesta!',
  thanksBody: 'Tu respuesta ha sido registrada.',
  passwordGateSubtitle: 'Este formulario está protegido con contraseña. Ingresa la contraseña para continuar.',
  passwordLabel: 'Contraseña',
  passwordChecking: 'Comprobando…',
  passwordUnlock: 'Desbloquear',
  passwordIncorrect: 'Contraseña incorrecta. Vuelve a intentarlo.',
  passwordRequired: 'Por favor ingresa la contraseña.',
  passwordChanged: 'La contraseña del formulario ha cambiado. Recarga la página y vuelve a ingresarla.',
  closedTitle: 'Este formulario ya no acepta respuestas.',
  closedGeneric: 'Este formulario ya no acepta respuestas.',
  previous: 'Anterior',
  next: 'Siguiente',
  submit: 'Enviar',
  submitting: 'Enviando…',
  saveForLater: 'Guardar y continuar más tarde',
  savingDraft: 'Guardando…',
  draftSavedTitle: 'Tu progreso se ha guardado',
  draftSavedBody: 'Vuelve con este enlace para continuar donde lo dejaste:',
  copyLink: 'Copiar enlace',
  done: 'Listo',
  draftNote: 'El enlace es válido durante 30 días. Guárdalo bien — cualquiera con el enlace puede ver o editar tu respuesta.',
  yourDetails: 'Tus datos (opcional)',
  yourName: 'Tu nombre',
  yourEmail: 'Tu correo',
  fixHighlightedContinue: 'Corrige los campos resaltados para continuar.',
  fixHighlightedSubmit: 'Corrige los campos resaltados y vuelve a enviar.',
  noLongerAcceptingResponses: 'Este formulario ya no acepta respuestas.',
  submitGenericError: 'Algo salió mal al enviar tu respuesta.',
  submittingTooQuickly: 'Estás enviando demasiado rápido. Espera un momento e inténtalo de nuevo.',
  duplicateEmail: 'Ya se envió una respuesta desde esta dirección de correo.',
  draftSaveError: 'No se pudo guardar tu borrador. Vuelve a intentarlo.',
  linkNotValid: 'Este enlace no es válido.',
};

const fr: Record<PublicStringKey, string> = {
  loading: 'Chargement du formulaire…',
  unavailable: 'Formulaire indisponible',
  thanksTitle: 'Merci pour votre réponse !',
  thanksBody: 'Votre réponse a bien été enregistrée.',
  passwordGateSubtitle: 'Ce formulaire est protégé par un mot de passe. Entrez le mot de passe pour continuer.',
  passwordLabel: 'Mot de passe',
  passwordChecking: 'Vérification…',
  passwordUnlock: 'Déverrouiller',
  passwordIncorrect: 'Mot de passe incorrect. Veuillez réessayer.',
  passwordRequired: 'Veuillez saisir le mot de passe.',
  passwordChanged: 'Le mot de passe du formulaire a changé. Rechargez la page et saisissez-le à nouveau.',
  closedTitle: 'Ce formulaire n\'accepte plus de réponses.',
  closedGeneric: 'Ce formulaire n\'accepte plus de réponses.',
  previous: 'Précédent',
  next: 'Suivant',
  submit: 'Envoyer',
  submitting: 'Envoi en cours…',
  saveForLater: 'Enregistrer et continuer plus tard',
  savingDraft: 'Enregistrement…',
  draftSavedTitle: 'Votre progression est enregistrée',
  draftSavedBody: 'Revenez avec ce lien pour reprendre là où vous en étiez :',
  copyLink: 'Copier le lien',
  done: 'Terminé',
  draftNote: 'Le lien est valide 30 jours. Gardez-le en sécurité — toute personne disposant du lien peut consulter ou modifier votre réponse.',
  yourDetails: 'Vos coordonnées (facultatif)',
  yourName: 'Votre nom',
  yourEmail: 'Votre adresse e-mail',
  fixHighlightedContinue: 'Veuillez corriger les champs signalés pour continuer.',
  fixHighlightedSubmit: 'Veuillez corriger les champs signalés et envoyer à nouveau.',
  noLongerAcceptingResponses: 'Ce formulaire n\'accepte plus de réponses.',
  submitGenericError: 'Une erreur est survenue lors de l\'envoi de votre réponse.',
  submittingTooQuickly: 'Vous envoyez trop rapidement. Attendez un instant et réessayez.',
  duplicateEmail: 'Une réponse a déjà été envoyée depuis cette adresse e-mail.',
  draftSaveError: 'Impossible d\'enregistrer votre brouillon. Veuillez réessayer.',
  linkNotValid: 'Ce lien n\'est pas valide.',
};

export const PUBLIC_STRINGS: Record<PublicLocale, Record<PublicStringKey, string>> = {
  en, es, fr,
};

export const LOCALE_OPTIONS: Array<{ value: PublicLocale; label: string }> = [
  { value: 'en', label: 'English' },
  { value: 'es', label: 'Español' },
  { value: 'fr', label: 'Français' },
];

// Resolves a raw locale string (which may include a region like "en-US") to
// a supported PublicLocale, or falls back to English. Missing keys inside a
// supported locale also fall back to English.
export function t(rawLocale: string | null | undefined, key: PublicStringKey): string {
  const base = (rawLocale ?? 'en').split('-')[0].toLowerCase() as PublicLocale;
  const dict = PUBLIC_STRINGS[base] ?? PUBLIC_STRINGS.en;
  return dict[key] ?? PUBLIC_STRINGS.en[key] ?? '';
}
