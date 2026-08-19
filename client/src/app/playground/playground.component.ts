import { ChangeDetectionStrategy, Component, signal } from '@angular/core';
import { FbFormComponent, FbSubmitEvent } from 'formbuilder-renderer';

// Demo page for the formbuilder-renderer library: paste an exported form
// JSON, hit Render, and get a working form — the same thing a customer's
// own Angular app does with <fb-form [schema]>. Nothing here is persisted.
@Component({
  selector: 'app-playground',
  standalone: true,
  imports: [FbFormComponent],
  templateUrl: './playground.component.html',
  styleUrl: './playground.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PlaygroundComponent {
  jsonText = signal('');
  schema = signal<unknown>(null);
  parseError = signal<string | null>(null);
  lastSubmission = signal<string | null>(null);

  render(): void {
    this.lastSubmission.set(null);
    const text = this.jsonText().trim();
    if (!text) {
      this.parseError.set('Paste a form JSON first.');
      this.schema.set(null);
      return;
    }
    try {
      // Only JSON syntax is checked here; schema shape errors are reported by
      // <fb-form> itself, so the library stays the single source of truth.
      this.schema.set(JSON.parse(text));
      this.parseError.set(null);
    } catch (err) {
      this.schema.set(null);
      this.parseError.set(err instanceof Error ? err.message : 'Invalid JSON.');
    }
  }

  onFile(event: Event): void {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) return;
    file.text().then((text) => {
      this.jsonText.set(text);
      this.render();
    });
  }

  onSubmitted(event: FbSubmitEvent): void {
    this.lastSubmission.set(JSON.stringify(event.data, null, 2));
  }
}
