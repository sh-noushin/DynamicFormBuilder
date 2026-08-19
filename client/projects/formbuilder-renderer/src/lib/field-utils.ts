// Field-level helpers shared by the renderer. Every function here mirrors the
// behaviour of FormBuilder's own public form page, so a form renders the same
// whether it is served from /f/{slug} or from an exported JSON file.

import { ValidatorFn, Validators } from '@angular/forms';
import { FbField, FbFieldOption } from './form-schema';

export interface FbValidationRules {
  minimum?: number;
  maximum?: number;
  rangeMessage?: string;
  maxFileSizeMb?: number;
  allowedFileExtensions?: string[];
  // Set when the field's Validation column holds a bare regex rather than a
  // rules object — the older format still present in seeded forms.
  pattern?: string;
}

// Control names are derived from the field name (not an id) because exports
// deliberately omit ids. Field names are unique within a form: they are the
// keys of the submission payload.
export function controlNameFor(field: FbField): string {
  return `field_${field.name}`;
}

export function parseValidation(raw?: string | null): FbValidationRules | null {
  if (!raw) return null;
  const trimmed = raw.trim();
  if (!trimmed) return null;
  if (trimmed.startsWith('{')) {
    try {
      return JSON.parse(trimmed) as FbValidationRules;
    } catch {
      return null;
    }
  }
  return { pattern: trimmed };
}

export function parseOptions(field: FbField): FbFieldOption[] {
  if (!field.options) return [];
  try {
    const parsed = JSON.parse(field.options);
    if (!Array.isArray(parsed)) return [];
    return parsed.map((opt: unknown) => {
      if (typeof opt === 'string') return { value: opt, label: opt };
      const o = opt as Partial<FbFieldOption>;
      return { value: o.value ?? '', label: o.label ?? o.value ?? '' };
    });
  } catch {
    return [];
  }
}

// Checkbox is excluded from `required` on purpose: an unchecked box is a
// legitimate answer, matching how the server validates submissions.
export function validatorsFor(field: FbField): ValidatorFn[] {
  const validators: ValidatorFn[] = [];
  if (field.isRequired && field.type !== 'Checkbox') {
    validators.push(Validators.required);
  }
  if (field.type === 'Email') {
    validators.push(Validators.email);
  }

  const rules = parseValidation(field.validation);
  if (rules) {
    if (typeof rules.minimum === 'number') validators.push(Validators.min(rules.minimum));
    if (typeof rules.maximum === 'number') validators.push(Validators.max(rules.maximum));
    if (rules.pattern) {
      try {
        validators.push(Validators.pattern(new RegExp(rules.pattern)));
      } catch {
        // An unparseable regex shouldn't block the whole form.
      }
    }
  }

  return validators;
}

export function initialValueFor(field: FbField): unknown {
  if (field.type === 'Checkbox') {
    return /^(true|1|yes|on)$/i.test((field.defaultValue ?? '').trim());
  }
  return field.defaultValue ?? '';
}

// Evaluates showIfCondition against current values.
// Format: {"field":"otherFieldName","equals":"value"}. A missing or
// unparseable rule means the field is always visible — never hide content
// because of malformed metadata.
export function isFieldVisible(
  field: FbField,
  valueByFieldName: (name: string) => unknown
): boolean {
  if (field.isVisible === false) return false;
  if (!field.showIfCondition) return true;

  let rule: { field?: string; equals?: unknown } | null = null;
  try {
    rule = JSON.parse(field.showIfCondition);
  } catch {
    return true;
  }
  if (!rule?.field) return true;

  const actual = valueByFieldName(rule.field);
  return String(actual ?? '') === String(rule.equals ?? '');
}

// Human-readable error for the first failing validator on a control.
export function errorMessageFor(field: FbField, errors: Record<string, unknown> | null): string | null {
  if (!errors) return null;
  if (errors['required']) return `${field.label || field.name} is required.`;
  if (errors['email']) return 'Enter a valid email address.';

  const rules = parseValidation(field.validation);
  if (errors['min'] || errors['max']) {
    if (rules?.rangeMessage) return rules.rangeMessage;
    const min = rules?.minimum;
    const max = rules?.maximum;
    if (typeof min === 'number' && typeof max === 'number') {
      return `Enter a value between ${min} and ${max}.`;
    }
    if (errors['min']) return `Value must be at least ${min}.`;
    return `Value must be at most ${max}.`;
  }
  if (errors['pattern']) return `${field.label || field.name} is not in the expected format.`;
  return 'This value is not valid.';
}
