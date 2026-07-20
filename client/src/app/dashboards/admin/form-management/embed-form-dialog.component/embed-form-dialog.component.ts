import { Component, Inject, computed, signal, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';

export type EmbedFormDialogData = {
  formName: string;
  publicUrl: string;
};

// Constrains height to sane values so a copy-paste iframe isn't accidentally
// 1 pixel tall or 100000 pixels tall.
const MIN_HEIGHT = 200;
const MAX_HEIGHT = 4000;

@Component({
  selector: 'app-embed-form-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatButtonToggleModule,
    MatSnackBarModule,
  ],
  templateUrl: './embed-form-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrls: ['./embed-form-dialog.component.scss'],
})
export class EmbedFormDialogComponent {
  widthMode = signal<'full' | 'fixed'>('full');
  widthPx = signal<number>(640);
  heightPx = signal<number>(800);

  // Fed straight into the <iframe [src]> preview. Router already sanitizes
  // this in practice but MatDialog templates need a SafeResourceUrl.
  safeUrl: SafeResourceUrl;

  constructor(
    private dialogRef: MatDialogRef<EmbedFormDialogComponent>,
    private snack: MatSnackBar,
    sanitizer: DomSanitizer,
    @Inject(MAT_DIALOG_DATA) public data: EmbedFormDialogData,
  ) {
    this.safeUrl = sanitizer.bypassSecurityTrustResourceUrl(data.publicUrl);
  }

  widthAttr = computed<string>(() =>
    this.widthMode() === 'full' ? '100%' : String(this.clampWidth(this.widthPx()))
  );

  snippet = computed<string>(() => {
    const w = this.widthAttr();
    const h = this.clampHeight(this.heightPx());
    // Double quotes around attribute values match what MDN/HTML5 boilerplate
    // uses so the paste target is as boring/portable as possible.
    return `<iframe src="${this.data.publicUrl}" width="${w}" height="${h}" style="border:0;max-width:100%;" title="${this.escapeAttr(this.data.formName)}"></iframe>`;
  });

  // Live preview iframe dimensions - clamped to whatever the widthAttr resolves
  // to, so switching between full/fixed updates the preview immediately.
  previewWidth = computed<string>(() => this.widthAttr() === '100%' ? '100%' : `${this.widthAttr()}px`);
  previewHeight = computed<number>(() => this.clampHeight(this.heightPx()));

  setWidthMode(mode: 'full' | 'fixed'): void {
    this.widthMode.set(mode);
  }
  setWidthPx(value: string): void {
    const n = Number(value);
    if (Number.isFinite(n)) this.widthPx.set(n);
  }
  setHeightPx(value: string): void {
    const n = Number(value);
    if (Number.isFinite(n)) this.heightPx.set(n);
  }

  copySnippet(): void {
    const text = this.snippet();
    if (navigator.clipboard?.writeText) {
      navigator.clipboard.writeText(text).then(
        () => this.snack.open('Embed code copied', 'Close', { duration: 2000 }),
        () => this.snack.open('Copy failed', 'Close', { duration: 3000 }),
      );
    } else {
      this.snack.open('Clipboard API not available', 'Close', { duration: 3000 });
    }
  }

  close(): void { this.dialogRef.close(); }

  private clampWidth(v: number): number {
    if (!Number.isFinite(v)) return 640;
    return Math.max(200, Math.min(1600, Math.round(v)));
  }
  private clampHeight(v: number): number {
    if (!Number.isFinite(v)) return 800;
    return Math.max(MIN_HEIGHT, Math.min(MAX_HEIGHT, Math.round(v)));
  }
  private escapeAttr(s: string): string {
    return s.replace(/&/g, '&amp;').replace(/"/g, '&quot;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
  }
}
