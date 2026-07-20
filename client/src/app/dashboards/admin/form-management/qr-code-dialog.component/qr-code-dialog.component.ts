import { ChangeDetectionStrategy, Component, Inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import * as QRCode from 'qrcode';

export interface QrCodeDialogData {
  formName: string;
  publicUrl: string;
}

@Component({
  selector: 'app-qr-code-dialog',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.Eager,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatSnackBarModule,
  ],
  templateUrl: './qr-code-dialog.component.html',
  styleUrls: ['./qr-code-dialog.component.scss'],
})
export class QrCodeDialogComponent implements OnInit {
  svg = signal<SafeHtml>('');
  // Raw SVG markup we hand back for the download button. Kept separate from
  // the SafeHtml projection because a Blob needs the original string.
  private rawSvg = '';

  constructor(
    private sanitizer: DomSanitizer,
    private snack: MatSnackBar,
    private dialogRef: MatDialogRef<QrCodeDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: QrCodeDialogData,
  ) {}

  async ngOnInit(): Promise<void> {
    try {
      // Standard error-correction level M is a good default: readable when
      // ~15% of the code is obscured (poster wear, camera glare) without
      // blowing up the size. 256px matches the dialog frame.
      this.rawSvg = await QRCode.toString(this.data.publicUrl, {
        type: 'svg',
        errorCorrectionLevel: 'M',
        margin: 1,
        width: 256,
      });
      // The qrcode lib emits a self-contained <svg> we can inline directly.
      this.svg.set(this.sanitizer.bypassSecurityTrustHtml(this.rawSvg));
    } catch (e) {
      this.snack.open('Could not generate QR code', 'Close', { duration: 3000 });
    }
  }

  copyUrl(): void {
    if (!navigator.clipboard?.writeText) {
      this.snack.open('Clipboard API not available', 'Close', { duration: 3000 });
      return;
    }
    navigator.clipboard.writeText(this.data.publicUrl).then(
      () => this.snack.open('Link copied', 'Close', { duration: 2000 }),
      () => this.snack.open('Copy failed', 'Close', { duration: 3000 }),
    );
  }

  downloadSvg(): void {
    if (!this.rawSvg) return;
    const safeName = this.data.formName.replace(/[^a-z0-9-_ ]/gi, '_').trim() || 'form';
    const blob = new Blob([this.rawSvg], { type: 'image/svg+xml' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `${safeName}-qr.svg`;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
  }

  close(): void { this.dialogRef.close(); }
}
