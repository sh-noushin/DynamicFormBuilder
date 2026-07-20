import {
  AfterViewInit,
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  EventEmitter,
  Input,
  OnDestroy,
  Output,
  ViewChild,
  signal,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-signature-pad',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.Eager,
  imports: [CommonModule, MatButtonModule, MatIconModule],
  templateUrl: './signature-pad.component.html',
  styleUrl: './signature-pad.component.scss',
})
export class SignaturePadComponent implements AfterViewInit, OnDestroy {
  @ViewChild('canvas', { static: true }) canvasRef!: ElementRef<HTMLCanvasElement>;
  @Input() width = 480;
  @Input() height = 160;
  @Input() strokeColor = '#0f172a';
  @Input() strokeWidth = 2;
  @Input() label = '';
  @Output() valueChange = new EventEmitter<string | null>();

  hasSignature = signal(false);

  private ctx: CanvasRenderingContext2D | null = null;
  private drawing = false;
  private lastX = 0;
  private lastY = 0;
  private pointerId: number | null = null;

  ngAfterViewInit(): void {
    const canvas = this.canvasRef.nativeElement;
    canvas.width = this.width;
    canvas.height = this.height;
    this.ctx = canvas.getContext('2d');
    if (this.ctx) {
      this.ctx.lineCap = 'round';
      this.ctx.lineJoin = 'round';
      this.ctx.strokeStyle = this.strokeColor;
      this.ctx.lineWidth = this.strokeWidth;
      this.ctx.fillStyle = '#ffffff';
      this.ctx.fillRect(0, 0, canvas.width, canvas.height);
    }
    canvas.addEventListener('pointerdown', this.onPointerDown);
    canvas.addEventListener('pointermove', this.onPointerMove);
    canvas.addEventListener('pointerup', this.onPointerUp);
    canvas.addEventListener('pointercancel', this.onPointerUp);
    canvas.addEventListener('pointerleave', this.onPointerUp);
  }

  ngOnDestroy(): void {
    const canvas = this.canvasRef?.nativeElement;
    if (!canvas) return;
    canvas.removeEventListener('pointerdown', this.onPointerDown);
    canvas.removeEventListener('pointermove', this.onPointerMove);
    canvas.removeEventListener('pointerup', this.onPointerUp);
    canvas.removeEventListener('pointercancel', this.onPointerUp);
    canvas.removeEventListener('pointerleave', this.onPointerUp);
  }

  clear(): void {
    if (!this.ctx) return;
    const canvas = this.canvasRef.nativeElement;
    this.ctx.fillStyle = '#ffffff';
    this.ctx.fillRect(0, 0, canvas.width, canvas.height);
    this.hasSignature.set(false);
    this.valueChange.emit(null);
  }

  private onPointerDown = (event: PointerEvent) => {
    if (!this.ctx) return;
    event.preventDefault();
    this.drawing = true;
    this.pointerId = event.pointerId;
    const { x, y } = this.pointOf(event);
    this.lastX = x;
    this.lastY = y;
    this.canvasRef.nativeElement.setPointerCapture(event.pointerId);
  };

  private onPointerMove = (event: PointerEvent) => {
    if (!this.drawing || !this.ctx || event.pointerId !== this.pointerId) return;
    event.preventDefault();
    const { x, y } = this.pointOf(event);
    this.ctx.beginPath();
    this.ctx.moveTo(this.lastX, this.lastY);
    this.ctx.lineTo(x, y);
    this.ctx.stroke();
    this.lastX = x;
    this.lastY = y;
  };

  private onPointerUp = (event: PointerEvent) => {
    if (!this.drawing || event.pointerId !== this.pointerId) return;
    this.drawing = false;
    this.pointerId = null;
    try { this.canvasRef.nativeElement.releasePointerCapture(event.pointerId); } catch { /* ignore */ }
    if (!this.hasSignature()) this.hasSignature.set(true);
    this.emitCurrent();
  };

  private emitCurrent(): void {
    const dataUrl = this.canvasRef.nativeElement.toDataURL('image/png');
    this.valueChange.emit(dataUrl);
  }

  private pointOf(event: PointerEvent): { x: number; y: number } {
    const rect = this.canvasRef.nativeElement.getBoundingClientRect();
    return {
      x: (event.clientX - rect.left) * (this.width / rect.width),
      y: (event.clientY - rect.top) * (this.height / rect.height),
    };
  }
}
