import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CreateVersionDialogComponent } from './create-version-dialog.component';

describe('CreateVersionDialogComponent', () => {
  let component: CreateVersionDialogComponent;
  let fixture: ComponentFixture<CreateVersionDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CreateVersionDialogComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CreateVersionDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
